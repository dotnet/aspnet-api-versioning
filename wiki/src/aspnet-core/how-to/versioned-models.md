# Versioned Models

When an API is versioned, it is often necessary to version the models that are used in the API. This is especially true
when shared models are used in request and response messages.

[Asp.Versioning.Abstractions] provides the `[VisibleInApiVersion]` attribute to indicate which API versions a model is
visible in. When no attribute is applied, the model is visible in all APIs. The abstractions library does not have any
dependency on ASP.NET and carries no additional dependencies. Any intended use case is referencing abstractions in
libraries that provide version-specific metadata.

Consider the following model:

```c#
public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    [VisibleInApiVersion( "2.0" )]
    public string MiddleName { get; set; }

    public string LastName { get; set; }

    [VisibleInApiVersion( "2.0" )]
    public string Email { get; set; }

    [VisibleInApiVersion( "3.0" )]
    public string Phone { get; set; }
}
```

The `Person` model indicates that:

- `Id`, `FirstName`, and `LastName` are visible in all API versions
- `MiddleName` and `Email` are only visible starting in API version `2.0`
- `Phone` is only visible starting in API version `3.0`

Each value passed to the `[VisibleInApiVersion]` attribute is a range expression representing a rule set. The rule is
parsed into a range that determines if the annotated member applies to an API version. Annotations never define any
API versions.

In rare cases, you might need a split range. The `[VisibleInApiVersion]` attribute supports multiple entries; for example:

```c#
public class ExperimentalSettings
{
    [VisibleInApiVersion( "[,2.0)", "(2.0,]" )]
    public bool IsEnabled { get; set; }
}
```

These rules would express that `IsEnabled` is included in every API version **except** `2.0`.

## Notation

The interval notation for version ranges is as follows: 

| Notation  | Applied Rule  | Description                                           |
| --------- | ------------- | ----------------------------------------------------- |
| 1.0	    | x ≥ 1.0	    | Minimum version, inclusive                            |
| [1.0,)	| x ≥ 1.0	    | Minimum version, inclusive                            |
| (1.0,)	| x > 1.0	    | Minimum version, exclusive                            |
| [1.0]	    | x == 1.0	    | Exact version match                                   |
| (,1.0]	| x ≤ 1.0	    | Maximum version, inclusive                            |
| (,1.0)	| x < 1.0	    | Maximum version, exclusive                            |
| [1.0,2.0] | 1.0 ≤ x ≤ 2.0	| Exact range, inclusive                                |
| (1.0,2.0) | 1.0 < x < 2.0	| Exact range, exclusive                                |
| [1.0,2.0) | 1.0 ≤ x < 2.0	| Mixed inclusive minimum and exclusive maximum version |
| (1.0)	    | invalid	    | invalid                                               |

[Asp.Versioning.Abstractions]: https://nuget.org/packages/Asp.Versioning.Abstractions

## Validation

>[!IMPORTANT]
>This feature is currently only available for JSON content.

When a versioned API receives a request the deserialization process will enforce that a client did not _over-post_
more data than is allowed for the requested API version, even if the backing model defines the corresponding property.
If a client attempts to post data that is not visible in the requested API version, the request will be rejected with
HTTP status code `400` (Bad Request). The response body will indicate which properties were not visible in the
requested API version. This is the same behavior as if the property did not exist on the model at all.

## API Explorer

The API Explorer will look for and respect annotations; specifically, `IAnnotation<T, ApiVersionRange>`. The explored
API descriptions will only include models and properties that are visible in the API version being explored.

The OpenAPI extensions will leverage this information to generate version-specific OpenAPI documents with constrained
model properties. A client will not be able to tell whether you used a single model behind the scenes or many. From
their perspective, each model will appear to be unique with its own affinity the API version that defined it.