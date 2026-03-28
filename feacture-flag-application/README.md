# Feature Flag Application

A ASP.NET Core feature flag system with in-memory storage, user/group overrides, and runtime evaluation.

## Run it

1. Restore/build
   `dotnet build`
2. Start app
   `dotnet run`
3. Open Swagger
   `https://localhost:7127/swagger`

## Sample payload requests

Create feature flag:

POST `/api/featureflags`
```json
{
  "key": "new-ui",
  "enabled": false,
  "description": "New UI rollout"
}
```

Update feature flag:

PUT `/api/featureflags/new-ui`
```json
{
  "key": "new-ui",
  "enabled": true,
  "description": "Enable for everyone"
}
```

Set user override:

POST `/api/featureflags/new-ui/override/user?userId=user123&enabled=true`
(no JSON body)

Set group override:

POST `/api/featureflags/new-ui/override/group?groupId=beta&enabled=true`
(no JSON body)

Evaluate:

POST `/api/featureflags/new-ui/evaluate?userId=user123&groupId=beta`
(no JSON body)

Delete user override:

DELETE `/api/featureflags/new-ui/override/user?userId=user123`

Delete group override:

DELETE `/api/featureflags/new-ui/override/group?groupId=beta`

Delete feature:

DELETE `/api/featureflags/new-ui`

### Assumptions made

- Used in-memory storage to ship quickly (no external DB dependency).
- Feature key is case-insensitive and trimmed.
- Override key uses (flag, subject) identity with value equality.
- User override has higher priority than group override, then global default.

### Tradeoffs chosen (and why)

- In-memory store: simple and fast for PoC, but not durable.
- Singleton store: safe for single app instance and allows shared runtime state.
- Limited validation: ensures required fields defined, no advanced structure rules.

### What I’d do next with another hour/day

- Add persistence-backed store support (EF Core + SQLite/PostgreSQL) with DI toggle.
- Add request/response DTOs and validation attributes for better API schema.
- Add structured logging and metrics for evaluate path.
- Add user/group membership service to evaluate group resolution key.

### Known limitations / rough edge

- No persistence; restart flushes all flags and overrides.
- Group override applies only when groupId provided (no group membership logic).
- Key normalization is case-insensitive for flag name, but user/group IDs are case-sensitive currently.
- No authentication/authorization on feature management endpoints.
