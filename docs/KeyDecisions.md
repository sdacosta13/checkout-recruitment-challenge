
## Functional requirements

The API described in the README needs to accept requests from users for payments, it should validate then forward the request onto the acquiring bank and store the result.
The client should then be informed of the result.

As this is a payments gateway, it is critical that double charging of customers are not possible. Additionally, it should be thoroughly tested so that there is no unexpected behaviour.
Since this service sits in front of other services it should be tolerant to downstream services going down, and there should be a define process for this event.

The API should also be a well documented REST api for end users so they are able to easily integrate and make payments. Swagger should be used to aid this

## Non functional requirements

- DTO's should be used when communicating between services in order to decouple versioning issues.
- The API should be observable, a minimum would be reasonable logging. Better would be live monitoring provided through services like DataDog or application insights. 
An uptime monitor like gatus would also be useful
- Structured logging should be used to aid observability 
- Failures communicating to backend services should be retried with an exponential backoff policy in order to lessen demand on services when there are transient failures
- Idempotency keys can be used to prevent duplicate payments.
- The API should be tolerant to concurrent requests - This can be an issue as we are using a singleton repository.
- The service should have some mechanism to determine it is like i.e a simple health check.
- Given that I have been provided a docker based simulator, I might as well use this in test via Test Containers.
- Problem details should be used to provide clients with consistent responses

## Production Gaps
# Storage
The repository is currently in memory which means all data is lots on restart.
Additionally, the idempotency store has no TTL, so it will grow infinitely. Also, the idempotency store should be external to the service. 
The storage should be seperated from the API so that the application can be horizontally scaled.


# Observability 
Currently, logs/requests do not have tracing ids properly configure, which would make supporting the application tricky.
There are default trace ids from ASP.NET Core, but they have not been forwarded to the bank account client. This would make it difficult for downstream requests.

# Retry policy 
Currently, all HttpRequestExceptions are retried. These should probably be filtered so that we only retry on transient errors that we can detect (via status code)
Additionally any 400's are currently retried which is rather pointless as it's unlikely to change

# Security 
No authentication or authorisation is currently implemented. Any user can currently see any payments made by other users.
No rate limiting.

# Operational Readiness
Health checks for downstream services would be needed
No graceful shutdown handling - Inflight requests may be interrupted.





