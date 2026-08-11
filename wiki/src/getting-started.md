<!-- description: Install ASP.NET API Versioning and stand up a versioned Hello World endpoint in a few lines of code. -->

# Getting Started

The simplest way to get started is to install the library.

```bash
dotnet add package Asp.Versioning.Http
```

## Example

The following example sets up the ubiquitous "Hello World" service with two versions of the same endpoint. The `version`
parameter resolves to the request API version and echoes it back to illustrate different endpoints were reached.

```c#
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var helloworld = app.NewVersionedApi().MapGroup("/helloworld");
var v1 = helloworld.MapGroup("/").HasApiVersion(1.0);
var v2 = helloworld.MapGroup("/").HasApiVersion(2.0);

// GET /helloworld?api-version=1.0
v1.MapGet("/", (ApiVersion version) => $"Hello World! (v{version})");

// GET /helloworld?api-version=2.0
v2.MapGet("/", (ApiVersion version) => $"Hello World! (v{version})");

app.Run();
```

To run the example, use:

```bash
dotnet run
```

and then navigate to the endpoint or use:

```bash
curl https://localhost:5001/helloworld?api-version=1.0
```

```bash
curl https://localhost:5001/helloworld?api-version21.0
```