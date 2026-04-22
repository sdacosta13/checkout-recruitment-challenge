# Key Decisions

## Functional Requirements

The API accepts payment requests from merchants, validates them, forwards them to the acquiring bank, stores the result, and returns the outcome to the caller.

As a payments gateway, preventing double-charging is critical. The service must be thoroughly tested and tolerant of downstream failures, with a defined behaviour when the bank is unreachable.

The API is documented as a REST API using Swagger to aid merchant integration.

## Non-Functional Requirements

| Concern           | Decision                                                                                                                                     |
|-------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| **Decoupling**    | DTOs are used at all service boundaries to decouple internal versioning from the API contract                                                |
| **Observability** | Serilog with structured logging and request middleware; production would add DataDog/Application Insights and an uptime monitor (e.g. Gatus) |
| **Idempotency**   | Clients may supply an `Idempotency-Key` header; duplicate requests with the same key return the cached response without re-charging          |
| **Concurrency**   | All singleton services use `ConcurrentDictionary` to handle concurrent requests safely                                                       |
| **Resilience**    | Failures calling the bank are retried with exponential backoff to reduce pressure during transient outages                                   |
| **Consistency**   | Problem Details (RFC 7807) is used for all error responses                                                                                   |
| **Health**        | A `/health` endpoint is registered for liveness checks                                                                                       |
| **Testing**       | The Docker-based bank simulator is used in integration tests via Testcontainers, giving confidence against the real simulator behaviour      |

## Production Gaps

### Storage

The repository is in-memory, so all data is lost on restart. The idempotency store also has no TTL and will grow indefinitely. Both stores should be externalised (e.g. a database and Redis) so the application can be horizontally scaled and data survives restarts.

### Observability

Trace IDs from ASP.NET Core are not forwarded to the bank client, making it difficult to correlate gateway logs with downstream requests. A proper distributed tracing setup (e.g. OpenTelemetry) would be needed in production.

### Retry Policy

All `HttpRequestException`s are currently retried, including bank 4xx responses which are unlikely to succeed on a retry. The policy should filter to only retry on transient errors detectable by status code (e.g. 503, 429).

### Security

No authentication or authorisation is implemented. Any caller can retrieve any payment by ID. Rate limiting is also absent.

### Operational Readiness

- Health checks do not probe downstream dependencies (e.g. the bank or database)
- No graceful shutdown handling — in-flight requests may be interrupted on pod termination
