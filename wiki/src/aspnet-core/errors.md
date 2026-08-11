<!-- description: The RFC 7807 problem details returned for API version errors in ASP.NET Core. -->

{{#include ../shared/errors-pre.md}}

## Customization

Error responses can be customized or extended in a variety of ways. You must opt into using problem details via:

```c#
services.AddProblemDetails();
```

>[!NOTE]
>Applies to .NET 7+

If problem details are not added, clients will receive an error response which only has the HTTP status code. You might
choose this approach if you don't want a response body or your error responses do not comply with RFC 7807.

To modify the way a problem is written to clients, you can implement and register a your own `IProblemDetailsWriter`
implementation. Each registered implementation is injected into the `IProblemDetailsService`. The first matching writer
is used to write the response body. For more information see the ASP.NET Core [Problem Details] documentation.


The `IProblemDetailsService` was not added to support `ProblemDetails` in Minimal APIs and MVC Core until .NET 7. In API
Versioning `6.x`, the `IProblemDetailsFactory` interface was used to bridge this gap. Contrary to the opt-in behavior of
`AddProblemDetails()`, a default implementation of `IProblemDetailsFactory` is automatically registered for Minimal
APIs. If MVC Core is added, then a decorated adapter is automatically provided over `ProblemDetailsFactory`. You have
the choice of replacing the entire `IProblemDetailsFactory` service or the MVC Core specific `ProblemDetailsFactory`.

The `IProblemDetailsFactory` interface was completely removed in .NET 7+ because it is no longer used in any way.

>[!IMPORTANT]
>Applies to .NET 6

## Backward Compatibility

While it is possible to customize error responses and retain the previous **Error Object** format, there is considerable
work required to enable this behavior and may block adoption of new library versions. Additional extensions have been
added to retain backward compatibility or continue to use **Error Objects** if you so desire.

Using **Error Object** responses is as simple as registering the `ErrorObjectWriter` to emit them. The critical part of
each setup is the order in which the writer is registered. If the writer is not registered in the correct order, it will
not be selected. Each configuration **must** occur before `AddApiVersioning()`.

The default implementation of the `ErrorObjectWriter` **only** writes **Error Objects** for API versioning related
errors. The default ASP.NET Core behavior provided by `AddProblemDetails()` is used for writing other types of errors.
If you want to use **Error Objects** for other error responses, you can extend `ErrorObjectWriter` and override which
types of Problem Details it should match - perhaps all of them.

>[!NOTE]
>Applies to 8.1.0+

#### Minimal API

`AddErrorObjects()` adds the default behavior; however, you can register a custom `ErrorObjectWriter` via
`AddErrorObject<TWriter>()`. Both methods allow a custom `Action<JsonOptions>` setup and will configure the default
behavior if not otherwise specified.

```c#
builder.Services.AddProblemDetails().AddErrorObjects();
builder.Services.ApiVersioning();
```

#### MVC (Core)

```c#
builder.Services.AddControllers();
builder.Services.AddErrorObjects().AddProblemDetails();
builder.Services.ApiVersioning().AddMvc();
```

>[!NOTE]
>Applies to 7.1.0+

#### Minimal API

```c#
builder.Services.AddProblemDetails();
builder.Services.TryAddEnumerable( ServiceDescriptor.Singleton<IProblemDetailsWriter, ErrorObjectWriter>() );
builder.Services.ApiVersioning();
```

#### MVC (Core)

```c#
builder.Services.AddControllers();
builder.Services.TryAddEnumerable( ServiceDescriptor.Singleton<IProblemDetailsWriter, ErrorObjectWriter>() );
builder.Services.AddProblemDetails();
builder.Services.ApiVersioning().AddMvc();
```

>[!NOTE]
>Applies to .NET 6 and 6.5.0+

Since `IProblemDetailsService` did not exist in .NET 6, you must instead replace `IProblemDetailsFactory` with the
`ErrorObjectFactory` service. The configuration process and order are the same regardless of whether you are using
Minimal APIs or controllers. The replaced service should occur before `AddApiVersioning()`.

```c#
builder.Services.AddSingleton<IProblemDetailsFactory, ErrorObjectFactory>();
builder.Services.ApiVersioning();
```

[Problem Details]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling#problem-details