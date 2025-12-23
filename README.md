# User Story 1  
## Reliable Email Dispatch with Queue, DLQ, and Idempotency (No Retry Scheduler)

**Project Name:** Email.Dispatcher  
**Title:** Send emails asynchronously with reliable delivery and no duplicates

---

## Tech Stack (Must Follow)
- **RabbitMQ**
  - Main Queue: `email.dispatcher.send`
  - Dead Letter Queue (DLQ): `email.dispatcher.dlq`
- **.NET Worker Service**
  - `Email.Dispatcher` (RabbitMQ Consumer + Email Sender)
- **Database (SQL Server or equivalent)**
  - `EmailLog` table (email state, attempts, errors)
  - `EmailIdempotency` table (prevents duplicate sends)
- **SMTP / Email Provider**
  - SendGrid / SMTP / SES (provider adapter based)

---

## Business Goal
As a system, I want to send emails (OTP, notifications, invoices, etc.) asynchronously so that:
- API responses are fast
- emails are not lost
- duplicates are prevented
- failures are captured and moved to DLQ
- operations teams can track and replay failed emails

---

## Actors
- End User (triggers actions requiring email)
- API (creates email request and publishes message)
- `Email.Dispatcher` – .NET Worker Service (RabbitMQ consumer)
- RabbitMQ (Main queue + DLQ)
- Database (EmailLog + Idempotency tables)
- Ops/Admin (monitor and replay failed emails)

---

## Functional Flow

### 1) Create Email Request (API)
**Given** a user action requires an email  
**When** the API processes the request  
**Then** the API must:

#### Database
- Insert **one row per email** into `EmailLog` with:
  - `Status = Pending`
  - `AttemptCount = 0`
  - `MessageKey` (unique idempotency key)
  - `CreatedAt = NOW`

#### Idempotency
- Insert into `EmailIdempotency`:
  - `MessageKey`
  - `EmailId`
- Enforce unique constraint on `MessageKey`

#### RabbitMQ
- Publish **one message per EmailId** to:
  - `email.dispatcher.send`
- Message payload must contain:
  - `EmailId`
  - `MessageKey`

#### Response
- Return success without waiting for email to be sent

**Acceptance Criteria**
- API must not send emails directly
- One email record = one RabbitMQ message
- If publish fails after DB save, email remains recoverable
- Email record must exist before sending is attempted

---

### 2) Send Email  
(**Email.Dispatcher – RabbitMQ Consumer / Worker Service**)

**Given** a message is consumed from `email.dispatcher.send`  
**When** the worker processes the message  
**Then** it must:

#### Read
- Load `EmailLog` by `EmailId`

#### Idempotency Check (Mandatory)
- If `Status = Sent` → ACK and stop
- If `MessageKey` already completed → ACK and stop

#### Locking / Lease
- Acquire DB lease:
  - `LockedUntil = NOW + LeaseDuration`
- Only one worker may send the email

#### Send
- Send email using configured SMTP/provider

##### On Success
- Update DB:
  - `Status = Sent`
  - `SentAt = NOW`
  - Clear `LastError`
- Mark idempotency as completed
- ACK RabbitMQ message

##### On Failure
- Update DB:
  - `Status = Failed`
  - `AttemptCount = AttemptCount + 1`
  - `LastError = <summary>`
- Publish message to DLQ: `email.dispatcher.dlq`
- ACK original message

**Acceptance Criteria**
- At-least-once delivery must not cause duplicates
- Multiple worker instances must be safe
- Failures must never block the main queue
- DLQ message must contain enough data for replay

---

### 3) Dead Letter Handling (DLQ)
**Given** email sending fails  
**When** failure is detected  
**Then**:
- Message is published to `email.dispatcher.dlq`
- Email remains visible in DB with failure reason

**Acceptance Criteria**
- DLQ is the source for inspection and replay
- Ops/Admin can re-publish EmailId after fixing issues

---

## Non-Functional Requirements

### Idempotency
- `MessageKey` must be unique per logical email
- Idempotency table must prevent duplicate sends
- Duplicate RabbitMQ deliveries must be safe

### Locking / Concurrency
- DB lease (`LockedUntil`) prevents double sends
- Lease expiry allows recovery if worker crashes

### Observability
- Log every attempt:
  - EmailId
  - Attempt number
  - Error type
- Metrics:
  - Pending
  - Failed
  - Dead (DLQ)
  - Sent per hour/day

### Scalability
- Multiple worker instances supported
- RabbitMQ distributes messages automatically
- Throughput increases by adding workers

---

## Definition of Done
- API inserts EmailLog + EmailIdempotency
- API publishes one message per EmailId
- Email.Dispatcher consumes and sends emails
- Idempotency guarantees no duplicates
- Failures are sent to DLQ
- Logs and metrics available
- Retry scheduler explicitly excluded
