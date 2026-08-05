### Configuration

The configuration will then change the default API version reader as follows:

```c#
.AddApiVersioning( options => options.ApiVersionReader = new MediaTypeApiVersionReader() );
```

The parameterless constructor uses the media type parameter name `v`, but you can specify any name you like. The default
behavior will require that clients always specify an API version, so service authors will likely want their
configuration to be:

```c#
.AddApiVersioning(
    options =>
    {
        options.ApiVersionReader = new MediaTypeApiVersionReader();
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
    } );
```

This will allow clients to request a specific API version by media type, but if they don't specify anything, they will
receive the current implementation (e.g. API version). For example:

```http
GET api/helloworld HTTP/2
host: localhost
```
_Figure 1: returns the result from API version 2.0 because it's the current version_

```http
GET api/helloworld HTTP/2
host: localhost
accept: text/plain;v=1.0
```
_Figure 2: returns the result from API version 1.0_

```http
POST api/helloworld HTTP/2
host: localhost
content-type: text/plain;v=2.0
content-length: 12

Hello there!
```
_Figure 3: explicitly posts the content to API version 2.0, even though it would be implicitly matched_

## Multiple Media Types

The `MediaTypeApiVersionReader` matches the configured media type parameter of **any** incoming request. This might be
undesirable if you support multiple media types or there is ambiguity in matching a media type.

Consider the following request:

```http
GET api/helloworld HTTP/2
host: localhost
accept: application/json;v=1.0;q=0.8,application/signed-exchange;v=b3;q=0.9
```

In this scenario, a client has specified multiple media types and they both have the media type parameter `v`. The
`MediaTypeApiVersionReader` will honor quality (e.g. `q`) when specified. If multiple media types have the same quality,
the first one is selected. In this example `application/signed-exchange` is selected because it has the highest quality.
When the `v` parameter is parsed, the value is `b3` is not a valid API version and will return HTTP status code `406`
(Not Acceptable).

The `MediaTypeApiVersionReaderBuilder` provides a number of additional capabilities to build media type matching rules
that enable to you configure how you would like things to match. You can specify and combine any of the following
behaviors:

- Define multiple media type parameters
- Mutually include specific media types
- Mutually exclude specific media types
- Match media types by template
- Match media types by pattern
- Disambiguate between multiple API versions

To configure that only JSON be matched, you might use a configuration similar to the following:

```c#
.AddApiVersioning(
    options =>
    {
        var builder = new MediaTypeApiVersionReaderBuilder();

        options.ApiVersionReader = builder.Parameter( "v" )
                                          .Include( "application/json" )
                                          .Build();
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
    } );
```

An important difference between `MediaTypeApiVersionReaderBuilder` and `MediaTypeApiVersionReader` is that
`MediaTypeApiVersionReader` expects there to be exactly one API version and selects the first one with the highest
_quality_. The `MediaTypeApiVersionReaderBuilder`, on the other hand, makes no such assumption and returns all matched
API versions in descending order of _quality_. You can use the `SelectFirstOrDefault` or `SelectLastOrDefault` extension
methods to have the `MediaTypeApiVersionReaderBuilder` choose the first or last API version respectively. If neither of
these approaches meet your requirements, you can provide you own callback to determine how to disambiguate multiple
choices via `MediaTypeApiVersionReaderBuilder.Select`.

## Custom Media Types

Defining new, custom media types (ex: `application/vnd.my.company.1+json`) to drive API versioning is another variant of
this approach that is compliant with the constraints of REST. There is no specific `IApiVersionReader` meant to address
this scenario, however, the `MediaTypeApiVersionReaderBuilder` provides two approaches that can be used.

### Templates

The most natural approach is to a use a template to match an API version in the media type. The specified template uses
the same syntax and matching as a route template. For example,

```c#
.AddApiVersioning(
    options =>
    {
        var builder = new MediaTypeApiVersionReaderBuilder();

        options.ApiVersionReader = builder.Template( "application/vnd.my.company.{version}+json" )
                                          .Build();
    } );
```

This allows matching the API version the same way as if it were in a URL segment. All of the same format and parsing
rules apply. In most cases, this is sufficient; however, the template expects **exactly one** parameter and that will be
assumed to the API version parameter. If there are multiple route parameters, for whatever reason, the expected name
must be provided as the second, optional parameter:

```c#
Template( "application/vnd.{tenant}.{version}+json", "version" );
```

### Patterns

If a template will not suffice, then a regular expression pattern can be used.

```c#
.AddApiVersioning(
    options =>
    {
        var builder = new MediaTypeApiVersionReaderBuilder();

        options.ApiVersionReader = builder.Match( @"-v(\d+(\.\d+)?)\+" ).Build();
    } );
```

`MediaTypeApiVersionReaderBuilder.Match` will **only** consider the first match. The match may optionally use grouping,
but only the first regular expression group will be considered. If a requested media type does not match the pattern,
then it is ignored.

It is assumed that your pattern matching requirements will fall under the date (e.g. group) or numeric version formats;
however, if you have something more complex, the following pattern will match all forms of a valid API version:

```regex
^(\d{4}-\d{2}-\d{2})?\.?(\d{0,9})\.?(\d{0,9})\.?-?(.*)$
```

API Versioning no longer uses regular expressions to parse API versions; however, if you need to know how this can be
used from previous implementations, you can review the [old code].

### Additional Considerations

While using a template or pattern can be used to match and extract an API version from an incoming request, it does not
currently provide any additional support that may be need to implement a full solution. These should be known issues and
exist even without API Versioning. You should simply beware that API Versioning isn't providing any additional features
beyond matching the API version from the media type in the incoming request.

[old code]: https://github.com/dotnet/aspnet-api-versioning/blob/0612bbd32f39b2607cf64e86fc8892d19e39dce7/src/Common/ApiVersion.cs#L182