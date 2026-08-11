<!-- description: Configure API versioning in ASP.NET Core with top-level statements or the Startup class. -->

# Configuring Your Application

Although different variations of ASP.NET have distinct application initialization methods, careful consideration was
taken to make the API versioning configuration as similar as possible across all applications models.

Two methods of configuration are supported. All examples will use the new top-level statements method, but the older
`Startup.cs` method is still supported.

## Top-Level Statements

```c#

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddMvc(); // ← brings in MVC Core; unnecessary for Minimal APIs

// remaining setup omitted for brevity
```

## Startup

The configuration for ASP.NET Core applications typically occur in the `ConfigureServices` method of the **Startup.cs**
file. To enable API versioning support with the default options, use the following configuration:

```c#
public void ConfigureServices( IServiceCollection services )
{
    services.AddControllers();
    services.AddProblemDetails();
    services.AddApiVersioning()
            .AddMvc(); // ← brings in MVC Core; unnecessary for Minimal APIs

    // remaining setup omitted for brevity
}
```