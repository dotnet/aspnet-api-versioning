namespace ApiVersioning.Examples.Services;

using Asp.Versioning.Builder;

/// <summary>
/// Represents the base builder implementation for a verisoned API.
/// </summary>
/// <param name="builder">The underlying <see cref="IVersionedEndpointRouteBuilder">versioned endpoint route builder</see>.</param>

public abstract class VersionedApiBuilder( IVersionedEndpointRouteBuilder builder )
{
    /// <summary>
    /// Gets versioned endpoint route builder.
    /// </summary>
    public IVersionedEndpointRouteBuilder Endpoints => builder;
}

/// <summary>
/// Represents a versioned API builder with a model type.
/// </summary>
/// <typeparam name="T">The associated model type.</typeparam>
/// <param name="builder">The underlying <see cref="IVersionedEndpointRouteBuilder">versioned endpoint route builder</see>.</param>
public sealed class VersionedApiBuilder<T>( IVersionedEndpointRouteBuilder builder ) : VersionedApiBuilder( builder )
{
    /// <summary>
    /// Gets the associated model type.
    /// </summary>
    public Type ModelType => typeof( T );

    /// <summary>
    /// Gets the associated type for a sequence of model types.
    /// </summary>
    public Type EnumerableModelType = typeof( IEnumerable<T> );
}

/// <summary>
/// Represents the Orders API builder.
/// </summary>
/// <param name="builder">The underlying <see cref="IVersionedEndpointRouteBuilder">versioned endpoint route builder</see>.</param>
public sealed class OrdersApiBuilder( IVersionedEndpointRouteBuilder builder ) : VersionedApiBuilder( builder );

/// <summary>
/// Represents the People API builder.
/// </summary>
/// <param name="builder">The underlying <see cref="IVersionedEndpointRouteBuilder">versioned endpoint route builder</see>.</param>
public sealed class PeopleApiBuilder( IVersionedEndpointRouteBuilder builder ) : VersionedApiBuilder( builder );