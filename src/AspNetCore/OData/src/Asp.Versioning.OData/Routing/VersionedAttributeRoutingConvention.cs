// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;

/// <summary>
/// Represents an API version-aware OData <see cref="AttributeRoutingConvention"/>.
/// </summary>
[CLSCompliant( false )]
public class VersionedAttributeRoutingConvention : AttributeRoutingConvention
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
    /// </summary>
    /// <param name="logger">The registered logger.</param>
    /// <param name="parser">The registered parser.</param>
    public VersionedAttributeRoutingConvention(
        ILogger<AttributeRoutingConvention> logger,
        IODataPathTemplateParser parser )
        : base( logger, parser ) { }

    /// <inheritdoc />
    public override bool AppliesToAction( ODataControllerActionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var metadata = context.Action
                              .Selectors
                              .SelectMany( s => s.EndpointMetadata.OfType<ApiVersionMetadata>() )
                              .FirstOrDefault();

        if ( metadata == null )
        {
            return false;
        }

        if ( metadata.IsApiVersionNeutral )
        {
            NormalizeAttributeRouteTemplates( context );
            return base.AppliesToAction( context );
        }

        if ( ( context.Model ?? FindModel( context ) ) is not IEdmModel edm )
        {
            return false;
        }

        var apiVersion = edm.GetApiVersion();

        if ( apiVersion == null || !metadata.IsMappedTo( apiVersion ) )
        {
            return false;
        }

        NormalizeAttributeRouteTemplates( context );
        return base.AppliesToAction( context );
    }

    // REF: https://github.com/OData/AspNetCoreOData/blob/main/src/Microsoft.AspNetCore.OData/Routing/Conventions/AttributeRoutingConvention.cs#L238
    private static string? FindRelatedODataPrefix( string routeTemplate, string[] prefixes )
    {
        if ( routeTemplate.StartsWith( '/' ) )
        {
            routeTemplate = routeTemplate[1..];
        }
        else if ( routeTemplate.StartsWith( "~/", StringComparison.Ordinal ) )
        {
            routeTemplate = routeTemplate[2..];
        }

        var hasEmptyPrefix = false;

        for ( var i = 0; i < prefixes.Length; i++ )
        {
            var prefix = prefixes[i];

            if ( prefix.Length == 0 )
            {
                hasEmptyPrefix = true;
            }
            else if ( routeTemplate.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
            {
                return prefix;
            }
        }

        return hasEmptyPrefix ? string.Empty : default;
    }

    private static bool IsODataController( ODataControllerActionContext context )
    {
        if ( ODataControllerSpecification.IsSatisfiedBy( context.Controller ) )
        {
            return true;
        }

        return ODataControllerSpecification.IsSatisfiedBy( context.Action );
    }

    private static IEdmModel? FindModel( ODataControllerActionContext context )
    {
        var routeComponents = context.Options.RouteComponents;

        if ( routeComponents.Count == 0 )
        {
            return default;
        }

        var prefixes = routeComponents.Keys.ToArray();
        var controllerModel = context.Controller;
        var controllerSelectors = controllerModel.Selectors.Where( sm => sm.AttributeRouteModel != null ).ToArray();
        var actionModel = context.Action;
        var actionSelectors = actionModel.Selectors;

        for ( var i = 0; i < actionSelectors.Count; i++ )
        {
            if ( controllerSelectors.Length == 0 )
            {
                var combinedRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        default,
                        actionSelectors[i].AttributeRouteModel );
                var template = combinedRouteModel?.Template;

                if ( template != null && FindRelatedODataPrefix( template, prefixes ) is string prefix )
                {
                    return routeComponents[prefix].EdmModel;
                }
            }
            else
            {
                for ( var j = 0; j < controllerSelectors.Length; j++ )
                {
                    var controllerSelector = controllerSelectors[j];
                    var combinedRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        controllerSelector.AttributeRouteModel,
                        actionSelectors[i].AttributeRouteModel );
                    var template = combinedRouteModel?.Template;

                    if ( template != null && FindRelatedODataPrefix( template, prefixes ) is string prefix )
                    {
                        return routeComponents[prefix].EdmModel;
                    }
                }
            }
        }

        return default;
    }

    private static void NormalizeAttributeRouteTemplates( ODataControllerActionContext context )
    {
        if ( !IsODataController( context ) )
        {
            return;
        }

        var selectors = context.Action.Selectors;

        for ( var i = 0; i < selectors.Count; i++ )
        {
            if ( selectors[i].AttributeRouteModel is not AttributeRouteModel attributeRoute ||
                !attributeRoute.IsAbsoluteTemplate )
            {
                continue;
            }

            // HACK: if AttributeRoutingConvention visits the same descriptor more than once
            //       it will incorrectly prepend '/', which results in '//' and is an invalid
            //       route template pattern.
            //
            // REF: https://github.com/OData/AspNetCoreOData/blob/main/src/Microsoft.AspNetCore.OData/Routing/Conventions/AttributeRoutingConvention.cs#L181
            if ( attributeRoute.Template is string template && template.StartsWith( '/' ) )
            {
                attributeRoute.Template = template.TrimStart( '/' );
            }
        }
    }
}