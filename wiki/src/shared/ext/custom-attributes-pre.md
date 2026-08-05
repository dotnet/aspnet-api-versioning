# Attributes

In addition to the [API versioning options], there are few other customization and extension points. Attributes are the
primary mechanism used to decorate the API version metadata with a specific controller type, but the attributes used
can be any `IApiVersionProvider`.

```c#
public interface IApiVersionProvider
{
    ApiVersionProviderOptions Options { get; }
    IReadOnlyList<ApiVersion> Versions { get; }
}
```

There are several API version provider attributes defined out-of-the-box:

- `ApiVersionsBaseAttribute`
- `ApiVersionAttribute`
- `MapToApiVersionAttribute`
- `AdvertiseApiVersionsAttribute`

These attributes are themselves extensible. For example, you might choose to have your own attributes that are
unambiguously a specific version:

```c#
[AttributeUsage( AttributeUsage.Class, AllowMultiple = true, Inherited = false )]
public sealed class V1Attribute : ApiVersionAttribute
{
    public V1Attribute() : base( new ApiVersion( new( 2016, 7, 1 ) ) ) { }
}
```