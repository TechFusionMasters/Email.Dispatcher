# User Story 2  
## Provide Email.Dispatcher as an Installable Template

**Title:** Install email dispatch pipeline via command or Visual Studio template

---

## Goal
As a platform/dev-experience owner,  
I want to provide Email.Dispatcher as an installable template  
So that developers can add reliable email dispatch to their solution using a command or Visual Studio, with minimal changes.

---

## Scope
Template must support:
- Creating a new ready-to-run setup
- Adding Email.Dispatcher into an existing solution

---

## Acceptance Criteria
- Template is installable via:
  - dotnet new (primary)
  - Appears in Visual Studio templates (optional)
- Developers can run:
  - `dotnet new email-dispatcher --name MyApp`
  - `dotnet new email-dispatcher.add --existingSolution`

---

## Template Generates
- Email.Dispatcher – .NET Worker Service (Email Sender)
- Email.Dispatcher.RetryScheduler – .NET Worker Service
- DB scripts/migrations:
  - Email
  - Outbox / Attempts
- appsettings samples:
  - RabbitMQ
  - SMTP / email provider
- Short README:
  - What to configure
  - What not to change

---

## Developer Changes Required
- Connection string
- RabbitMQ configuration
- Email provider configuration

---

## Template Defaults
- Retry backoff policy
- Max retry attempts
- Idempotency guidance
- Fake email provider for local/dev mode

---

## Definition of Done
- Template published (internal feed or NuGet)
- Documented with:
  - Install command
  - Create-new command
  - Add-to-existing command
  - Minimal configuration checklist
