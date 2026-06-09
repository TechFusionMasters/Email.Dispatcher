# User Story 2  
## Automated Retry Scheduler for Failed Emails (DB-driven) with RabbitMQ Re-queueing

**Project Name:** Email.Dispatcher.RetryScheduler  
**Title:** Automatically retry failed emails using backoff and re-publish to RabbitMQ

---

## Tech Stack (Must Follow)
- **.NET Worker Service**
  - `Email.Dispatcher.RetryScheduler` (runs continuously)
- **Database (SQL Server or equivalent)**
  - `EmailLog` table (holds Status, AttemptCount, NextAttemptAt, errors)
  - Recommended index on `(Status, NextAttemptAt)`
- **RabbitMQ**
  - Main Queue: `email.dispatcher.send` (re-queue EmailId for resend)
  - Optional Retry Queue: `email.dispatcher.retry` (if you want separate routing)
- **Configuration**
  - `RetryIntervalSeconds` (default 60)
  - `MaxAttempts` (default 8)
  - `BackoffSchedule` (default: 1m, 5m, 15m, 1h, 3h, 6h, 12h, 24h)

---

## Business Goal
As a system, I want failed emails to be retried automatically so that:
- transient issues (SMTP outage, network issues) recover without manual effort
- customers receive emails eventually
- system avoids duplicate sends
- operations teams only handle true “dead” emails

---

## Actors
- Email.Dispatcher (sender worker that marks failures)
- Database (stores retry state)
- Email.Dispatcher.RetryScheduler (worker that schedules retries)
- RabbitMQ (re-queues EmailId messages)
- Ops/Admin (monitors Dead emails if retries exhausted)

---

## Functional Flow

### 1) Failure State Is Recorded (Pre-condition)
**Given** an email send attempt failed  
**When** Email.Dispatcher (sender worker) updates DB  
**Then** the email record must contain:
- `Status = Failed`
- `AttemptCount = AttemptCount + 1`
- `LastError = <summary>`
- `NextAttemptAt = NOW + Backoff(AttemptCount)`
- `LockedUntil = NULL` (or cleared)

**Acceptance Criteria**
- Every failed email must have a `NextAttemptAt` set
- Permanent failures should not be retried (marked `Dead` by sender worker or by separate policy)

---

### 2) Retry Scheduler Loop (Email.Dispatcher.RetryScheduler)
**Given** the RetryScheduler service is running  
**When** the scheduler interval elapses (every 30s / 1 min)  
**Then** it must:

#### Step A — Select Due Retries (DB)
Select only emails where:
- `Status = Failed`
- `NextAttemptAt <= NOW`
- `AttemptCount < MaxAttempts`
- `LockedUntil IS NULL OR LockedUntil < NOW` (if you use lease locking)

#### Step B — Claim Rows (Prevent double scheduling)
For selected rows, update them (atomic claim) to avoid multiple schedulers picking same rows:
- set `LockedUntil = NOW + LeaseDuration` (e.g., 2 minutes)
- optionally set `Status = RetryQueued`

#### Step C — Publish to RabbitMQ
For each claimed email:
- Publish message to `email.dispatcher.send` containing:
  - `EmailId`
  - `MessageKey`

#### Step D — Update DB after publish
- Set `Status = Pending` (or keep `RetryQueued` until sender picks it)
- Clear `LockedUntil` (or keep until send starts, based on your design)
- Store `LastQueuedAt = NOW` (optional)

**Acceptance Criteria**
- Scheduler must only pick “due” emails (no full table scan)
- Scheduler must never publish the same EmailId twice for the same attempt window
- Scheduler must be safe when multiple instances run (idempotent/claiming behavior)

---

### 3) Exhausted Retries → Mark as Dead
**Given** an email has reached MaxAttempts  
**When** scheduler detects `AttemptCount >= MaxAttempts`  
**Then** it must:
- Update DB:
  - `Status = Dead`
  - `DeadReason = MaxAttemptsExceeded`
  - keep `LastError`
- (Optional) Publish a DLQ/alert event for Ops visibility

**Acceptance Criteria**
- Emails exceeding MaxAttempts must not be retried automatically
- Dead emails must be visible for Ops/Admin manual action

---

## Retry Policy (Must Follow)
- Default `MaxAttempts = 8` (configurable)
- Default backoff schedule (configurable):
  - 1m → 5m → 15m → 1h → 3h → 6h → 12h → 24h
- Only transient failures should enter retry flow
- Permanent failures should be marked Dead (no retry)

---

## Non-Functional Requirements

### Reliability
- Scheduler must continue working after restarts (DB is source of truth)
- RabbitMQ publish failures must not lose retries:
  - if publish fails, keep email in `Failed` with `NextAttemptAt` moved forward (or keep same)

### Concurrency / Safety
- Scheduler must support multiple instances without duplicate scheduling
- Use DB claiming (lease) to prevent race conditions

### Observability
- Log every scheduling action:
  - EmailId
  - AttemptCount
  - NextAttemptAt
  - Published/Skipped reason
- Metrics:
  - due retries count
  - re-queued count
  - dead count (max attempts exceeded)

### Performance
- Required DB index:
  - `(Status, NextAttemptAt)` to query due retries efficiently

---

## Definition of Done
- RetryScheduler runs as .NET Worker Service continuously
- It picks only due failed emails and re-publishes EmailId to RabbitMQ
- Backoff and MaxAttempts are configurable
- Rows are claimed safely to avoid duplicate scheduling
- Emails exceeding retries are marked Dead
- Logs and basic metrics exist
