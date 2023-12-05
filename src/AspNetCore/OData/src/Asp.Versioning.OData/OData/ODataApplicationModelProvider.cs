// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning;
using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Controllers;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents an <see cref="IApplicationModelProvider">application model provider</see>, which
/// applies convention-based API versions controllers and their actions.
/// </summary>
[CLSCompliant( false )]
public class ODataApplicationModelProvider : IApplicationModelProvider
{
    private readonly IODataApiVersionCollectionProvider apiVersionCollectionProvider;
    private readonly IOptions<ApiVersioningOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApplicationModelProvider"/> class.
    /// </summary>
    /// <param name="conventionBuilder">The associated <see cref="IApiVersionConventionBuilder">convention builder</see>.</param>
    /// <param name="apiVersionCollectionProvider">The <see cref="IODataApiVersionCollectionProvider">provider</see> for OData-specific API versions.</param>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public ODataApplicationModelProvider(
        IApiVersionConventionBuilder conventionBuilder,
        IODataApiVersionCollectionProvider apiVersionCollectionProvider,
        IOptions<ApiVersioningOptions> options )
    {
        Order = BeforeOData;
        ConventionBuilder = conventionBuilder;
        this.apiVersionCollectionProvider = apiVersionCollectionProvider;
        this.options = options;
    }

    /// <inheritdoc />
    public int Order { get; protected set; }

    /// <summary>
    /// Gets the convention builder used by the application model provider.
    /// </summary>
    /// <value>The associated <see cref="IApiVersionConventionBuilder">convention builder</see>.</value>
    protected IApiVersionConventionBuilder ConventionBuilder { get; }

    /// <summary>
    /// Gets the associated API versioning options.
    /// </summary>
    /// <value>The configured <see cref="ApiVersioningOptions">API versioning options</see>.</value>
    protected ApiVersioningOptions Options => options.Value;

    private static int BeforeOData { get; } = ODataMultiModelApplicationModelProvider.DefaultODataOrder - 50;

    /// <inheritdoc />
    public virtual void OnProvidersExecuted( ApplicationModelProviderContext context ) { }

    /// <inheritdoc />
    public virtual void OnProvidersExecuting( ApplicationModelProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var (metadataControllers, supported, deprecated) = CollateApiVersions( context.Result );

        ApplyMetadataControllerConventions( metadataControllers, supported, deprecated );
        apiVersionCollectionProvider.ApiVersions = MergeApiVersions( supported, deprecated );
    }

    private static
        (
            List<ControllerModel>? MetadataControllers,
            SortedSet<ApiVersion>? SupportedApiVersions,
            SortedSet<ApiVersion>? DeprecatedApiVersions
        )
        CollateApiVersions( ApplicationModel application )
    {
        var controllers = application.Controllers;
        var metadataControllers = default( List<ControllerModel> );
        var supported = default( SortedSet<ApiVersion> );
        var deprecated = default( SortedSet<ApiVersion> );

        for ( var i = 0; i < controllers.Count; i++ )
        {
            var controller = controllers[i];

            if ( controller.ControllerType.IsMetadataController() )
            {
                metadataControllers ??= [];
                metadataControllers.Add( controller );
                continue;
            }
            else if ( !ODataControllerSpecification.IsSatisfiedBy( controller ) )
            {
                continue;
            }

            var actions = controller.Actions;

            for ( var j = 0; j < actions.Count; j++ )
            {
                var metadata = actions[j].Selectors
                                         .SelectMany( s => s.EndpointMetadata.OfType<ApiVersionMetadata>() )
                                         .FirstOrDefault();

                if ( metadata is null )
                {
                    continue;
                }

                var model = metadata.Map( Explicit );
                var versions = model.SupportedApiVersions;

                if ( supported == null && versions.Count > 0 )
                {
                    supported = [];
                }

                for ( var k = 0; k < versions.Count; k++ )
                {
                    supported!.Add( versions[k] );
                }

                versions = model.DeprecatedApiVersions;

                if ( deprecated == null && versions.Count > 0 )
                {
                    deprecated = [];
                }

                for ( var k = 0; k < versions.Count; k++ )
                {
                    deprecated!.Add( versions[k] );
                }
            }
        }

        return (metadataControllers, supported, deprecated);
    }

    private static ControllerModel? SelectBestMetadataController( IReadOnlyList<ControllerModel> controllers )
    {
        // note: there should be at least 2 metadata controllers, but there could be 3+
        // if a developer defines their own custom controller. ultimately, there can be
        // only one. choose and version the best controller using the following ranking:
        //
        // 1. VersionedMetadataController type (it's possible this has been removed upstream)
        // 2. original MetadataController type
        // 3. last, custom type of MetadataController from another assembly
        var bestController = default( ControllerModel );
        var original = typeof( MetadataController ).GetTypeInfo();
        var versioned = typeof( VersionedMetadataController ).GetTypeInfo();

        for ( var i = 0; i < controllers.Count; i++ )
        {
            var controller = controllers[i];

            if ( bestController == default )
            {
                bestController = controller;
            }
            else if ( bestController.ControllerType == original &&
                      controller.ControllerType == versioned )
            {
                bestController = controller;
            }
            else if ( bestController.ControllerType == versioned &&
                      controller.ControllerType != original )
            {
                bestController = controller;
            }
            else if ( bestController.ControllerType != versioned &&
                      controller.ControllerType != original )
            {
                bestController = controller;
            }
        }

        return bestController;
    }

    private void ApplyMetadataControllerConventions(
        List<ControllerModel>? metadataControllers,
        SortedSet<ApiVersion>? supported,
        SortedSet<ApiVersion>? deprecated )
    {
        if ( metadataControllers == null )
        {
            return;
        }

        var metadataController = SelectBestMetadataController( metadataControllers );

        if ( metadataController == null )
        {
            // graceful exit; in theory, this should never happen
            return;
        }

        if ( deprecated != null && supported != null )
        {
            deprecated.ExceptWith( supported );
        }

        var builder = ConventionBuilder.Controller( metadataController.ControllerType );

        if ( supported != null )
        {
            builder.HasApiVersions( supported );
        }

        if ( deprecated != null && deprecated.Count > 0 )
        {
            builder.HasDeprecatedApiVersions( deprecated );
        }

        builder.ApplyTo( metadataController );
    }

    private ApiVersion[] MergeApiVersions(
        SortedSet<ApiVersion>? supported,
        SortedSet<ApiVersion>? deprecated )
    {
        if ( deprecated == null )
        {
            if ( supported == null )
            {
                return [Options.DefaultApiVersion];
            }

            return [.. supported];
        }
        else if ( supported == null )
        {
            return [.. deprecated];
        }

        return supported.Union( deprecated ).ToArray();
    }
}