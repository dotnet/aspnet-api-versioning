# Copilot Instructions for ASP.NET API Versioning

## Project Overview

This is **ASP.NET API Versioning** (`dotnet/aspnet-api-versioning`), a .NET Foundation library providing API versioning semantics for ASP.NET. It produces multiple NuGet packages supporting ASP.NET Core (Minimal APIs, MVC, OData) and legacy ASP.NET Web API.

## Build & Test

- **SDK**: .NET 10.0 (`net10.0` primary; `net472` for legacy Web API; `netstandard1.0`/`2.0` for Abstractions)
- **Solution**: `asp.slnx` at repo root
- **Restore**: `dotnet restore`
- **Build**: `dotnet build` (or `dotnet build --configuration Release`)
- **Test**: `dotnet test` (or `dotnet test --configuration Release`)
- **Pack**: `dotnet pack --configuration Release`
- **Test runner**: Microsoft.Testing.Platform with xUnit v3 (`xunit.v3.mtp-v2`)

## Architecture

```
src/
├── Abstractions/       # Core types (ApiVersion, etc.) — multi-target netstandard1.0+
├── AspNetCore/
│   ├── WebApi/         # Asp.Versioning.Http, .Mvc, .Mvc.ApiExplorer, .OpenApi
│   ├── OData/          # Asp.Versioning.OData, .OData.ApiExplorer
│   └── Acceptance/     # Integration tests using TestHost
├── AspNet/
│   ├── WebApi/         # Asp.Versioning.WebApi, .WebApi.ApiExplorer (legacy)
│   ├── OData/          # Asp.Versioning.WebApi.OData (legacy)
│   └── Acceptance/     # Integration tests
├── Client/             # Asp.Versioning.Http.Client
└── Common/
    ├── src/            # Shared code via .shproj (Common, Common.Mvc, Common.OData, etc.)
    └── test/           # Tests for shared code
examples/               # Working example apps for each flavor
build/                  # MSBuild props/targets, CI YAML, signing key
```

- **Shared projects** (`.shproj`) under `src/Common/src/` contain code compiled into multiple assemblies. When modifying shared code, consider impact on all consuming projects.
- Production code lives in `src/**/src/`; tests live in `src/**/test/`.

## Code Conventions

- **C# Latest** language version with **implicit usings** enabled
- **Nullable reference types**: enabled in production code, disabled in tests
- **`var`**: preferred everywhere
- **`this.`**: avoid unless necessary (enforced as error)
- **Predefined type names**: use `int`, `string` over `Int32`, `String` (enforced as error)
- **Braces**: Allman style (new line before all braces)
- **Indentation**: 4 spaces for C#; 1 space for XML/MSBuild files
- **Line length**: 120 characters guideline
- **Properties**: expression-bodied preferred
- **Methods**: block body (not expression-bodied)
- **Analyzers**: StyleCop v1.2.0-beta with `TreatWarningsAsErrors: true` — fix all warnings before committing
- **Strong naming**: all production assemblies are signed (`build/key.snk`)
- **CLS Compliance**: production assemblies are CLS-compliant

## Test Conventions

- **Framework**: xUnit v3 with FluentAssertions and Moq
- **Class naming**: `[ClassName]Test` (e.g., `ApiVersionTest`, `SunsetPolicyManagerTest`)
- **Method structure**: arrange / act / assert (use comments to delineate sections)
- **Async tests**: use `TestContext.Current.CancellationToken`
- **Assertions**: FluentAssertions (`.Should().Be()`, `.Should().BeEquivalentTo()`, etc.)
- **Mocking**: Moq (`Mock.Of<>()`, `Mock.Get().Verify()`)
- **Every bug fix or feature must include tests**
- **Acceptance tests** use `TestHost` for full integration scenarios

## Build Infrastructure

- MSBuild property hierarchy flows through `build/*.props` → `src/Directory.Build.props` → individual `.csproj`
- NuGet package metadata is centralized in `build/nuget.props`
- Version is set per-project in `.csproj` files (currently `10.0.0`)
- CI runs on Azure Pipelines (`azure-pipelines.yml` → `build/steps-ci.yml`)
- Deterministic builds are enabled in CI (`ContinuousIntegrationBuild=true`)
- SourceLink is configured for GitHub

## Common Pitfalls

- Modifying `.shproj` shared projects affects multiple target assemblies — always build the full solution to verify
- The `.editorconfig` and StyleCop analyzers enforce strict style; the build fails on violations
- Legacy ASP.NET Web API projects (`net472`) have different API surfaces — test both frameworks when changing shared code
- Example projects have their own `Directory.Build.props` and `Directory.Packages.props` separate from `src/`
