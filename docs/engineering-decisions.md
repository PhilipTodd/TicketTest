---
layout: default
title: Engineering Decisions
---

# Engineering Decisions

The application deliberately favours a small, direct architecture rather than introducing patterns that are not justified by its current complexity.

## Keeping the Architecture Simple

The API does not introduce:

- A repository abstraction
- MediatR
- CQRS
- A separate application layer
- A separate domain-service layer

This was a deliberate engineering decision rather than an omission.

The application has a small domain model, straightforward persistence requirements and a limited set of business rules. Adding additional architectural layers would increase the number of abstractions, classes and execution paths without solving corresponding complexity in the application.

### Repository Pattern

Entity Framework Core's `DbContext` already provides repository-like and unit-of-work capabilities.

Introducing generic repositories around `DbContext` for this application would primarily wrap functionality already provided by EF Core and make queries less direct.

The controllers therefore use `AppDbContext` directly.

A repository or specialised data-access abstraction could become appropriate if persistence requirements became significantly more complex or if the application needed to isolate domain behaviour from multiple persistence mechanisms.

### MediatR and CQRS

Commands, queries and mediator handlers can provide useful separation in larger applications, particularly when operations have substantial independent workflows or cross-cutting behaviours.

The current ticket operations are small enough that introducing a mediator pipeline would add indirection without providing sufficient benefit.

Similarly, the application does not have sufficiently different read and write requirements to justify separate CQRS models.

These patterns could be introduced later if application complexity warranted them.

### Application and Domain Services

Business rules currently remain small and closely associated with ticket operations.

Introducing additional application-service and domain-service layers would therefore mostly move existing logic between classes rather than establish meaningful architectural boundaries.

The design can evolve toward those layers if business workflows become more sophisticated.

## Concurrency

The application implements version-based optimistic concurrency.

Each ticket contains a numeric `Version`.

When a client retrieves a ticket, the current version is returned with it. Updates and deletes submit that version back to the API.

Conceptually:

```text
Client retrieves ticket
        |
        +-- Version = 3
        |
        v
Client submits update
        |
        +-- Version = 3
        |
        v
API retrieves current ticket
        |
        +-- Version = 3 --> update accepted
        |                  Version becomes 4
        |
        +-- Version != 3 -> HTTP 409 Conflict
```

This prevents a client that is known to be working with stale data from silently overwriting a newer version.

### Limitation

The version comparison is performed by application code before `SaveChangesAsync()`.

It therefore does not provide fully database-enforced optimistic concurrency.

Two concurrent requests could theoretically:

1. Read the same ticket version.
2. Both pass the application-level version check.
3. Both attempt to update the record.

For a production system with stronger concurrency requirements, the SQL Server implementation would use an EF Core concurrency token, typically backed by SQL Server `rowversion`.

The database would then participate directly in the concurrency check, allowing EF Core to detect conflicting updates through `DbUpdateConcurrencyException`.

The simpler implementation was retained because it demonstrates the concurrency requirement without adding unnecessary complexity to the assessment-sized application.

## Testing Strategy

Integration tests exercise the API through HTTP rather than testing controller implementation details in isolation.

The deployed application uses Azure SQL, while integration tests use an isolated in-memory SQLite database.

This provides fast, repeatable tests without requiring external Azure infrastructure.

One limitation is that SQLite does not reproduce every SQL Server behaviour. Database-specific functionality should therefore be covered by SQL Server-based integration testing if the persistence layer becomes more sophisticated.

## Deliberately Not Over-Engineered

The project was originally implemented as a time-constrained technical assessment.

The goal was therefore not to demonstrate the maximum number of architectural patterns, but to implement the required behaviour clearly, test it, and make engineering trade-offs explicit.

The subsequent Azure deployment adds production-oriented infrastructure and delivery practices without unnecessarily restructuring the core application.