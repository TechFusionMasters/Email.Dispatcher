# User Story 1  
## Reliable Email Dispatch with Queue, Retries, DLQ, and Idempotency

**Project Name:** Email.Dispatcher  
**Title:** Send emails asynchronously with reliable retries and no duplicates

---

## Business Goal
As a system, I want to send emails (OTP, notifications, invoices, etc.) asynchronously so that:
- API responses are fast
- emails are not lost
- failures are retried automatically
- duplicates are prevented
- operations teams can track and replay failures

---

## Actors
- End User (triggers actions requiring email)
- API (creates email request)
- Email.Dispatcher – Email Sender Worker (RabbitMQ consumer)
- Email.Dispatcher – Retry Scheduler Worker
- Ops/Admin (monitor and replay failed emails)

---

## Functional Flow

### 1) Create Email Request (API)
**Given** a user action requires an email  
**When** the API processes the request  
**Then** the API must:
- Save an Email record in DB with:
  - Status = Pending
  - AttemptCount = 0
  - NextAttemptAt = NOW
  - MessageKey (unique idempotency key)
- Publish a message to RabbitMQ containing:
  - EmailId (and optionally MessageKey)
- Return success to the client without waiting for the email to send

**Acceptance Criteria**
- API must not send emails directly
- If publish fails after DB save, email must still be recoverable
- Email record must always exist before sending is attempted

---

### 2) Send Email  
(**Email.Dispatcher – RabbitMQ Consumer**)

**Given** a message is consumed from RabbitMQ with EmailId  
**When** the worker processes the message  
**Then** it must:
- Load the email row from DB
- Perform idempotency check:
  - If Status = Sent → acknowledge and stop
- Acquire DB lock/lease to avoid parallel sends
- Attempt to send email via SMTP/provider

**On Success**
- Update DB:
  - Status = Sent
  - SentAt = NOW
- Acknowledge message

**On Failure**
- Classify error:
  - Transient → retry
  - Permanent → dead
- Update DB retry fields
- Acknowledge message

**Acceptance Criteria**
- At-least-once delivery must not cause duplicates
- Multiple worker instances must be safe
- Queue must never block due to failures

---

### 3) Retry Handling  
(**Email.Dispatcher – Retry Scheduler**)

**Given** an email failed transiently  
**When** retry is required  
**Then** the system must:
- Update DB:
  - Status = Failed
  - AttemptCount = AttemptCount + 1
  - NextAttemptAt = NOW + Backoff(AttemptCount)
  - LastError
- Retry Scheduler runs every 30s / 1 min
- Select emails where:
  - Status = Failed
  - NextAttemptAt <= NOW
  - AttemptCount < MaxAttempts
- Publish EmailId back to RabbitMQ

**Acceptance Criteria**
- Scheduler must only process due retries
- Backoff must increase with attempts
- System must recover automatically after outages

---

### 4) Dead Letter Handling (DLQ)
**Given** an email is permanently failed or exceeded max attempts  
**When** retry decision is made  
**Then**:
- Mark email as:
  - Status = Dead
  - DeadReason
  - LastError
- Optionally publish to DLQ for alerting

**Acceptance Criteria**
- Dead emails must not retry automatically
- Ops/Admin must be able to view and replay emails

---

## Retry Policy
- MaxAttempts configurable (default: 8)
- Backoff example:
  - 1m → 5m → 15m → 1h → 3h → 6h → 12h → 24h
- Permanent failures (no retry):
  - invalid email
  - hard bounce
  - domain not found
  - policy blocked

---

## Non-Functional Requirements

### Idempotency
- MessageKey must be unique per logical email
- DB or status rules must prevent duplicates

### Locking / Concurrency
- DB lease (LockedUntil) must prevent double sends
- Lease expiry allows safe recovery after crashes

### Observability
- Log every attempt with:
  - EmailId
  - Attempt number
  - Error type
- Metrics:
  - Pending
  - Failed
  - Dead
  - Sent per hour/day

### Scalability
- Multiple consumers supported
- Retry scheduler must scale using indexed queries

---

## Definition of Done
- API creates email + publishes message
- Email.Dispatcher sends and updates DB
- Retry Scheduler re-queues due retries
- Dead emails are visible and replayable
- No duplicates under concurrency
- Logs and metrics available
