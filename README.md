# Second Brain

A microservices-based personal knowledge management (PKM) platform — capture notes, link ideas together, tag and organize them, search across them, and have them resurface again later instead of disappearing into a pile.

For the full system design (service breakdown, diagrams, event catalog, roadmap), see **[ARCHITECTURE.md](./ARCHITECTURE.md)**.

## Status

Early-stage. `Gateway` and `User` are implemented and runnable. `Brain` (the actual notes/tags/links domain) is scaffolded but not yet implemented. `Search` and `Notification` are planned. See the roadmap in `ARCHITECTURE.md` §8 for the build order.

## Tech stack

- .NET 10 / ASP.NET Core
- Entity Framework Core 10 + SQL Server
- Kafka (`Confluent.Kafka`) for domain events
- YARP for the API gateway
- JWT bearer auth, BCrypt password hashing
- xUnit + Moq + FluentAssertions for testing

## Solution layout

```
second-brain/
├── src/
│   ├── SecondBrain.BuildingBlocks/      # shared kernel: entities, repositories, Kafka, EF Core base
│   ├── SecondBrain.Gateway/             # YARP reverse proxy / API gateway
│   └── SecondBrain.Services/
│       ├── SecondBrain.Brain/           # notes, tags, links — scaffolded, not implemented
│       ├── SecondBrain.User/            # auth & identity
│       ├── SecondBrain.Search/          # planned
│       └── SecondBrain.Notification/    # planned
├── tests/
│   └── SecondBrain.User.Tests/
└── second-brain.slnx
```

## Getting started

Prerequisites: .NET 10 SDK, SQL Server (local or in a container), Kafka (only needed once a service actually publishes/consumes events — not required for `User` alone today).

```bash
# restore & build
dotnet restore
dotnet build

# apply migrations for the User service
dotnet ef database update --project src/SecondBrain.Services/SecondBrain.User/SecondBrain.User.csproj

# run the User service
dotnet run --project src/SecondBrain.Services/SecondBrain.User/SecondBrain.User.csproj

# run the Gateway
dotnet run --project src/SecondBrain.Gateway/SecondBrain.Gateway.csproj
```

Update the connection string in `src/SecondBrain.Services/SecondBrain.User/appsettings.Development.json` and the `Jwt:Secret` in `appsettings.json` before running anything outside of local dev — both currently ship with placeholder values checked into source.

## Running tests

```bash
dotnet test tests/SecondBrain.User.Tests/SecondBrain.User.Tests.csproj
```

## Contributing / next steps

If you're picking this up next, `SecondBrain.Brain` is the highest-value place to start — it's the only domain that's referenced everywhere (value objects, Kafka topics) but has no actual code yet. See `ARCHITECTURE.md` §8 for the suggested build order.