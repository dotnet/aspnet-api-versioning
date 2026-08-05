{{#include ../shared/errors-pre.md}}

## Customization

Error responses can be customized or extended in a variety of ways. RFC 7807 was ratified after active development on
ASP.NET Web API ceased. There are no out-of-the-box services provided. API Versioning provides a backport of the
`ProblemDetails` type as well as the `IProblemDetailsFactory`. The default implementation can be replaced by
implementing `IProblemDetailsFactory` and exposing it as a resolvable service via `HttpConfiguration.DependencyResolver`.

## Backward Compatibility

While it is possible to customize error responses and retain the previous **Error Object** format, there is
considerable work required to enable this behavior and may block adoption of new library versions. Additional extensions
have been added to retain backward compatibility or continue to use **Error Objects** if you so desire.

ASP.NET Web API does not provide an out-of-the-box dependency injection container; however, the following extension
method will wire up the necessary changes without having to add one of your own.

```c#
configuration.ConvertProblemDetailsToErrorObject();
```

>[!NOTE]
>Applies to 7.1.0+