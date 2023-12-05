// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.Controllers;
using Asp.Versioning.Conventions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class VersionedODataModelBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">HTTP configuration</see> associated with the builder.</param>
    /// <remarks>This constructor resolves the current <see cref="IHttpControllerSelector"/> from the
    /// <see cref="ServicesExtensions.GetHttpControllerSelector(ServicesContainer)"/> extension method via the
    /// <see cref="HttpConfiguration.Services"/>.</remarks>
    public VersionedODataModelBuilder( HttpConfiguration configuration ) => Configuration = configuration;

    /// <summary>
    /// Gets the associated HTTP configuration.
    /// </summary>
    /// <value>The <see cref="HttpConfiguration">HTTP configuration</see> associated with the builder.</value>
    protected HttpConfiguration Configuration { get; }

    /// <summary>
    /// Gets the API versioning options associated with the builder.
    /// </summary>
    /// <value>The configured <see cref="ApiVersioningOptions">API versioning options</see>.</value>
    protected ApiVersioningOptions Options => Configuration.GetApiVersioningOptions();

    /// <summary>
    /// Gets the API versions for all known OData routes.
    /// </summary>
    /// <returns>The <see cref="IReadOnlyList{T}">sequence</see> of <see cref="ApiVersion">API versions</see>
    /// for all known OData routes.</returns>
    protected virtual IReadOnlyList<ApiVersion> GetApiVersions()
    {
        var services = Configuration.Services;
        var assembliesResolver = services.GetAssembliesResolver();
        var typeResolver = services.GetHttpControllerTypeResolver();
        var actionSelector = services.GetActionSelector();
        var allControllerTypes = typeResolver.GetControllerTypes( assembliesResolver );
        var controllerTypes = new Type[allControllerTypes.Count];
        var controllerDescriptors = services.GetHttpControllerSelector().GetControllerMapping().Values.ToArray();
        var supported = default( SortedSet<ApiVersion> );
        var deprecated = default( SortedSet<ApiVersion> );

        allControllerTypes.CopyTo( controllerTypes, 0 );

        for ( var i = 0; i < controllerTypes.Length; i++ )
        {
            var controllerType = controllerTypes[i];

            if ( !controllerType.IsODataController() )
            {
                continue;
            }

            var controller = FindControllerDescriptor( controllerDescriptors, controllerType );

            if ( controller == null )
            {
                continue;
            }

            var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

            foreach ( var action in actions )
            {
                var model = action.GetApiVersionMetadata().Map( ApiVersionMapping.Explicit );
                var versions = model.SupportedApiVersions;

                if ( versions.Count > 0 && supported == null )
                {
                    supported = [];
                }

                for ( var j = 0; j < versions.Count; j++ )
                {
                    supported!.Add( versions[j] );
                }

                versions = model.DeprecatedApiVersions;

                if ( versions.Count > 0 && deprecated == null )
                {
                    deprecated = [];
                }

                for ( var j = 0; j < versions.Count; j++ )
                {
                    deprecated!.Add( versions[j] );
                }
            }
        }

        if ( deprecated != null && supported != null )
        {
            deprecated.ExceptWith( supported );
        }

        if ( ( supported == null || supported.Count == 0 ) &&
             ( deprecated == null || deprecated.Count == 0 ) )
        {
            ConfigureMetadataController( new[] { Options.DefaultApiVersion }, Enumerable.Empty<ApiVersion>() );
        }
        else
        {
            ConfigureMetadataController(
                supported ?? Enumerable.Empty<ApiVersion>(),
                deprecated ?? Enumerable.Empty<ApiVersion>() );
        }

        if ( supported == null )
        {
            if ( deprecated == null )
            {
                return Array.Empty<ApiVersion>();
            }

            return deprecated.ToArray();
        }
        else if ( deprecated == null )
        {
            return supported.ToArray();
        }

        supported.UnionWith( deprecated );
        return supported.ToArray();
    }

    /// <summary>
    /// Configures the metadata controller using the specified configuration and API versions.
    /// </summary>
    /// <param name="supportedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
    /// supported OData controller <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="deprecatedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
    /// deprecated OData controller <see cref="ApiVersion">API versions</see>.</param>
    protected virtual void ConfigureMetadataController(
        IEnumerable<ApiVersion> supportedApiVersions,
        IEnumerable<ApiVersion> deprecatedApiVersions )
    {
        var controllerMapping = Configuration.Services.GetHttpControllerSelector().GetControllerMapping();

        if ( !controllerMapping.TryGetValue( "VersionedMetadata", out var controllerDescriptor ) )
        {
            return;
        }

        var conventions = Options.Conventions;
        var controllerBuilder = conventions.Controller<VersionedMetadataController>()
                                           .HasApiVersions( supportedApiVersions )
                                           .HasDeprecatedApiVersions( deprecatedApiVersions );

        controllerBuilder.ApplyTo( controllerDescriptor );
    }

    private static HttpControllerDescriptor? FindControllerDescriptor(
        IReadOnlyList<HttpControllerDescriptor> controllerDescriptors,
        Type controllerType )
    {
        for ( var i = 0; i < controllerDescriptors.Count; i++ )
        {
            foreach ( var controllerDescriptor in controllerDescriptors[i].AsEnumerable() )
            {
                if ( controllerType.Equals( controllerDescriptor.ControllerType ) )
                {
                    return controllerDescriptor;
                }
            }
        }

        return default;
    }
}