// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System.Collections;
using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Represents a versioned <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public class VersionedEndpointRouteBuilder : IVersionedEndpointRouteBuilder
{
    private readonly IEndpointRouteBuilder routeBuilder;
    private readonly IEndpointConventionBuilder conventionBuilder;
    private readonly ServiceProviderDecorator serviceProvider;
    private readonly EndpointDataSourceCollectionAdapter dataSources;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedEndpointRouteBuilder"/> class.
    /// </summary>
    /// <param name="routeBuilder">The inner <see cref="IEndpointRouteBuilder"/> the new instance decorates.</param>
    /// <param name="conventionBuilder">The inner <see cref="IEndpointConventionBuilder"/> the new instance decorates.</param>
    /// <param name="apiVersionSetBuilder">The associated <see cref="ApiVersionSetBuilder">API version set builder</see>.</param>
    public VersionedEndpointRouteBuilder(
        IEndpointRouteBuilder routeBuilder,
        IEndpointConventionBuilder conventionBuilder,
        ApiVersionSetBuilder apiVersionSetBuilder )
    {
        this.routeBuilder = routeBuilder ?? throw new ArgumentNullException( nameof( routeBuilder ) );
        this.conventionBuilder = conventionBuilder ?? throw new ArgumentNullException( nameof( conventionBuilder ) );
        VersionSetBuilder = apiVersionSetBuilder ?? throw new ArgumentNullException( nameof( apiVersionSetBuilder ) );
        serviceProvider = new( routeBuilder.ServiceProvider, apiVersionSetBuilder );
        dataSources = new( routeBuilder.DataSources, apiVersionSetBuilder );
    }

    /// <summary>
    /// Gets the associated API version set builder.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionSet">API version set builder</see>.</value>
    protected ApiVersionSetBuilder VersionSetBuilder { get; }

    /// <inheritdoc />
    public virtual IApplicationBuilder CreateApplicationBuilder() =>
        routeBuilder.CreateApplicationBuilder();

    /// <inheritdoc />
    public virtual IServiceProvider ServiceProvider => serviceProvider;

    /// <inheritdoc />
    public virtual ICollection<EndpointDataSource> DataSources => dataSources;

    /// <inheritdoc />
    public virtual void Add( Action<EndpointBuilder> convention ) =>
        conventionBuilder.Add( convention );

    private sealed class ServiceProviderDecorator(
        IServiceProvider decorated,
        ApiVersionSetBuilder versionSetBuilder ) : IServiceProvider
    {
        private ApiVersionSet? versionSet;

        public object? GetService( Type serviceType )
        {
            if ( typeof( ApiVersionSetBuilder ).Equals( serviceType ) )
            {
                return versionSetBuilder;
            }

            if ( typeof( ApiVersionSet ).Equals( serviceType ) )
            {
                return versionSet ??= versionSetBuilder.Build();
            }

            return decorated.GetService( serviceType );
        }
    }

    private sealed class EndpointDataSourceDecorator(
            EndpointDataSource decorated,
            ApiVersionSetBuilder versionSetBuilder ) : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => decorated.Endpoints;

        public override IChangeToken GetChangeToken() => decorated.GetChangeToken();

        public override IReadOnlyList<Endpoint> GetGroupedEndpoints( RouteGroupContext context )
        {
            CollateGroupApiVersions();

            // HACK: we don't have a way to pass the version set for the group down
            // to each convention so decorate the service provider to allow it to
            // be resolved. this requires rebuilding the current context as well.
            if ( context.ApplicationServices is not ServiceProviderDecorator )
            {
                context = new()
                {
                    ApplicationServices = new ServiceProviderDecorator(
                        context.ApplicationServices,
                        versionSetBuilder ),
                    Conventions = context.Conventions,
                    FinallyConventions = context.FinallyConventions,
                    Prefix = context.Prefix,
                };
            }

            return decorated.GetGroupedEndpoints( context );
        }

        public override bool Equals( object? obj ) =>
            ReferenceEquals( this, obj ) || ReferenceEquals( decorated, obj );

        public override int GetHashCode() => decorated.GetHashCode();

        private void CollateGroupApiVersions()
        {
            // HACK: all conventions run for each endpoint in the group at a time; however, we
            // need to collate the api versions across all endpoints as part of the same
            // logical api. to retain this behavior, collate all advertised versions in group
            // into the associated version before any conventions run.
            var endpoints = Endpoints;

            for ( var i = 0; i < endpoints.Count; i++ )
            {
                var endpoint = endpoints[i];
                var metadata = endpoint.Metadata;

                for ( var j = 0; j < metadata.Count; j++ )
                {
                    if ( metadata[j] is not IApiVersionProvider provider ||
                         provider.Options.HasFlag( Mapped ) )
                    {
                        continue;
                    }

                    Func<ApiVersion, ApiVersionSetBuilder> add = provider.Options switch
                    {
                        None or Advertised => versionSetBuilder.AdvertisesApiVersion,
                        Deprecated or Advertised | Deprecated => versionSetBuilder.AdvertisesDeprecatedApiVersion,
                        _ => IgnoreApiVersion,
                    };

                    var versions = provider.Versions;

                    for ( var k = 0; k < versions.Count; k++ )
                    {
                        add( versions[k] );
                    }
                }
            }

            static ApiVersionSetBuilder IgnoreApiVersion( ApiVersion version ) => default!;
        }
    }

    private sealed class EndpointDataSourceCollectionAdapter(
            ICollection<EndpointDataSource> adapted,
            ApiVersionSetBuilder versionSetBuilder ) : ICollection<EndpointDataSource>
    {
        public int Count => adapted.Count;

        public bool IsReadOnly => adapted.IsReadOnly;

        public void Add( EndpointDataSource item ) =>
            adapted.Add( new EndpointDataSourceDecorator( item, versionSetBuilder ) );

        public void Clear() => adapted.Clear();

        public bool Contains( EndpointDataSource item ) => adapted.Contains( item );

        public void CopyTo( EndpointDataSource[] array, int arrayIndex ) =>
            adapted.CopyTo( array, arrayIndex );

        public IEnumerator<EndpointDataSource> GetEnumerator() => adapted.GetEnumerator();

        public bool Remove( EndpointDataSource item ) => adapted.Remove( item );

        IEnumerator IEnumerable.GetEnumerator() => adapted.GetEnumerator();
    }
}