// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using static System.StringComparison;

/// <summary>
/// Represents an OData attribute routing convention with additional support for API versioning.
/// </summary>
public class VersionedAttributeRoutingConvention : IODataRoutingConvention
{
    private const string AttributeRouteData = nameof( AttributeRouteData );
    private readonly ConcurrentDictionary<ApiVersion, IReadOnlyDictionary<ODataPathTemplate, HttpActionDescriptor>> attributeMappingsPerApiVersion = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
    public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration )
        : this( routeName, configuration, new DefaultODataPathHandler() ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
    /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
    public VersionedAttributeRoutingConvention(
        string routeName,
        HttpConfiguration configuration,
        IODataPathTemplateHandler pathTemplateHandler )
    {
        if ( string.IsNullOrEmpty( routeName ) )
        {
            throw new ArgumentNullException( nameof( routeName ) );
        }

        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        RouteName = routeName;
        ODataPathTemplateHandler = pathTemplateHandler ?? throw new ArgumentNullException( nameof( pathTemplateHandler ) );

        if ( pathTemplateHandler is IODataPathHandler pathHandler && pathHandler.UrlKeyDelimiter == null )
        {
            pathHandler.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
        }
    }

    /// <summary>
    /// Gets the name of the route associated the routing convention.
    /// </summary>
    /// <value>The name of the associated route.</value>
    public string RouteName { get; }

    /// <summary>
    /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
    /// </summary>
    /// <value>The <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.</value>
    public IODataPathTemplateHandler ODataPathTemplateHandler { get; }

    /// <summary>
    /// Returns a value indicating whether the specified controller should be mapped using attribute routing conventions.
    /// </summary>
    /// <param name="controller">The <see cref="HttpControllerDescriptor">controller descriptor</see> to evaluate.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
    /// <returns>True if the <paramref name="controller"/> should be mapped as an OData controller; otherwise, false.</returns>
    /// <remarks>The default implementation always returns <c>true</c>.</remarks>
    public virtual bool ShouldMapController( HttpControllerDescriptor controller, ApiVersion? apiVersion )
    {
        if ( controller == null )
        {
            throw new ArgumentNullException( nameof( controller ) );
        }

        var model = controller.GetApiVersionModel();
        return model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion );
    }

    /// <summary>
    /// Returns a value indicating whether the specified action should be mapped using attribute routing conventions.
    /// </summary>
    /// <param name="action">The <see cref="HttpActionDescriptor">action descriptor</see> to evaluate.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
    /// <returns>True if the <paramref name="action"/> should be mapped as an OData action or function; otherwise, false.</returns>
    /// <remarks>This method will match any OData action that explicitly or implicitly matches the API version applied
    /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
    public virtual bool ShouldMapAction( HttpActionDescriptor action, ApiVersion? apiVersion )
    {
        if ( action == null )
        {
            throw new ArgumentNullException( nameof( action ) );
        }

        return action.GetApiVersionMetadata().IsMappedTo( apiVersion );
    }

    /// <summary>
    /// Selects the controller for OData requests.
    /// </summary>
    /// <param name="odataPath">The OData path.</param>
    /// <param name="request">The request.</param>
    /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller.</returns>
    public virtual string? SelectController( ODataPath odataPath, HttpRequestMessage request )
    {
        if ( odataPath == null )
        {
            throw new ArgumentNullException( nameof( odataPath ) );
        }

        if ( request == null )
        {
            throw new ArgumentNullException( nameof( request ) );
        }

        if ( odataPath.Segments.Count == 0 )
        {
            return null;
        }

        var version = SelectApiVersion( request );
        var attributeMappings = attributeMappingsPerApiVersion.GetOrAdd( version, key => BuildAttributeMappings( key, request ) );
        var values = new Dictionary<string, object>();

        foreach ( var attributeMapping in attributeMappings )
        {
            var template = attributeMapping.Key;
            var action = attributeMapping.Value;

            if ( action.SupportedHttpMethods.Contains( request.Method ) && template.TryMatch( odataPath, values ) )
            {
                values[ODataRouteConstants.Action] = action.ActionName;
                request.Properties[AttributeRouteData] = values;

                return action.ControllerDescriptor.ControllerName;
            }
        }

        return null;
    }

    /// <summary>
    /// Selects the action for OData requests.
    /// </summary>
    /// <param name="odataPath">The OData path.</param>
    /// <param name="controllerContext">The controller context.</param>
    /// <param name="actionMap">The action map.</param>
    /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action.</returns>
    public virtual string? SelectAction(
        ODataPath odataPath,
        HttpControllerContext controllerContext,
        ILookup<string, HttpActionDescriptor> actionMap )
    {
        if ( odataPath == null )
        {
            throw new ArgumentNullException( nameof( odataPath ) );
        }

        if ( controllerContext == null )
        {
            throw new ArgumentNullException( nameof( controllerContext ) );
        }

        if ( actionMap == null )
        {
            throw new ArgumentNullException( nameof( actionMap ) );
        }

        var request = controllerContext.Request;
        var properties = request.Properties;

        if ( !properties.TryGetValue( AttributeRouteData, out var value ) || value is not IDictionary<string, object> attributeRouteData )
        {
            return null;
        }

        var routeData = request.GetRouteData();
        var routingConventionsStore = request.ODataProperties().RoutingConventionsStore;

        foreach ( var item in attributeRouteData )
        {
            if ( IsODataRouteParameter( item ) )
            {
                routingConventionsStore.Add( item );
            }
            else
            {
                routeData.Values.Add( item );
            }
        }

        return attributeRouteData[ODataRouteConstants.Action]?.ToString();
    }

    /// <summary>
    /// Selects the API version from the given HTTP request.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
    /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
    protected virtual ApiVersion SelectApiVersion( HttpRequestMessage request )
    {
        var version = request.GetRequestedApiVersionOrReturnBadRequest();

        if ( version != null )
        {
            return version;
        }

        var options = request.GetApiVersioningOptions();

        if ( !options.AssumeDefaultVersionWhenUnspecified )
        {
            return ApiVersion.Neutral;
        }

        var modelSelector = request.GetRequestContainer().GetRequiredService<IEdmModelSelector>();
        var versionSelector = request.GetApiVersioningOptions().ApiVersionSelector;
        var model = new ApiVersionModel( modelSelector.ApiVersions, Enumerable.Empty<ApiVersion>() );

        return versionSelector.SelectVersion( request, model );
    }

    private static IEnumerable<string> GetODataRoutePrefixes( IEnumerable<ODataRoutePrefixAttribute> prefixAttributes, string controllerName )
    {
        using var prefixAttribute = prefixAttributes.GetEnumerator();

        if ( !prefixAttribute.MoveNext() )
        {
            yield return string.Empty;
            yield break;
        }

        do
        {
            yield return GetODataRoutePrefix( prefixAttribute.Current, controllerName );
        }
        while ( prefixAttribute.MoveNext() );
    }

    private static string GetODataRoutePrefix( ODataRoutePrefixAttribute prefixAttribute, string controllerName )
    {
        var prefix = prefixAttribute.Prefix;

        if ( prefix != null && prefix.StartsWith( "/", Ordinal ) )
        {
            var message = string.Format( CultureInfo.CurrentCulture, SR.RoutePrefixStartsWithSlash, prefix, controllerName );
            throw new InvalidOperationException( message );
        }

        if ( prefix != null && prefix.EndsWith( "/", Ordinal ) )
        {
            prefix = prefix.TrimEnd( '/' );
        }

        return prefix ?? string.Empty;
    }

    private static bool IsODataRouteParameter( KeyValuePair<string, object> routeDatum )
    {
        // REF: https://github.com/OData/WebApi/blob/feature/netcore/src/Microsoft.AspNet.OData.Shared/Routing/ODataParameterValue.cs
        const string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_";
        const string ODataParameterValue = nameof( ODataParameterValue );

        return routeDatum.Key.StartsWith( ParameterValuePrefix, Ordinal ) && routeDatum.Value?.GetType().Name == ODataParameterValue;
    }

    private ODataPathTemplate? GetODataPathTemplate( string prefix, string pathTemplate, IServiceProvider serviceProvider )
    {
        if ( prefix != null && !pathTemplate.StartsWith( "/", Ordinal ) )
        {
            if ( string.IsNullOrEmpty( pathTemplate ) )
            {
                pathTemplate = prefix;
            }
            else if ( pathTemplate.StartsWith( "(", Ordinal ) )
            {
                pathTemplate = prefix + pathTemplate;
            }
            else
            {
                pathTemplate = prefix + "/" + pathTemplate;
            }
        }

        if ( pathTemplate.StartsWith( "/", Ordinal ) )
        {
            pathTemplate = pathTemplate.Substring( 1 );
        }

        return ODataPathTemplateHandler.SafeParseTemplate( pathTemplate, serviceProvider );
    }

    private static IEnumerable<string> GetODataRoutePrefixes( HttpControllerDescriptor controllerDescriptor )
    {
        var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
        return GetODataRoutePrefixes( prefixAttributes, controllerDescriptor.ControllerType.FullName );
    }

    private IReadOnlyDictionary<ODataPathTemplate, HttpActionDescriptor> BuildAttributeMappings(
        ApiVersion version,
        HttpRequestMessage request )
    {
        var configuration = request.GetConfiguration();
        var services = configuration.Services;
        var controllerSelector = services.GetHttpControllerSelector();
        var controllers = controllerSelector.GetControllerMapping().Values.ToArray();
        var attributeMappings = new Dictionary<ODataPathTemplate, HttpActionDescriptor>();
        var actionSelector = services.GetActionSelector();
        var serviceProvider = request.GetRequestContainer();

        for ( var i = 0; i < controllers.Length; i++ )
        {
            foreach ( var controller in controllers[i].AsEnumerable() )
            {
                if ( !controller.ControllerType.IsODataController() || !ShouldMapController( controller, version ) )
                {
                    continue;
                }

                var actionMapping = actionSelector.GetActionMapping( controller );
                var actions = actionMapping.SelectMany( a => a ).ToArray();

                foreach ( var prefix in GetODataRoutePrefixes( controller ) )
                {
                    for ( var j = 0; j < actions.Length; j++ )
                    {
                        var action = actions[j];

                        if ( !ShouldMapAction( action, version ) )
                        {
                            continue;
                        }

                        var pathTemplates = GetODataPathTemplates( prefix, action, serviceProvider );

                        foreach ( var pathTemplate in pathTemplates )
                        {
                            attributeMappings.Add( pathTemplate, action );
                        }
                    }
                }
            }
        }

        return attributeMappings;
    }

    private IEnumerable<ODataPathTemplate> GetODataPathTemplates(
        string prefix,
        HttpActionDescriptor action,
        IServiceProvider serviceProvider )
    {
        var routeAttributes = action.GetCustomAttributes<ODataRouteAttribute>( inherit: false );

        for ( var i = 0; i < routeAttributes.Count; i++ )
        {
            var pathTemplate = routeAttributes[i].PathTemplate;

            if ( GetODataPathTemplate( prefix, pathTemplate, serviceProvider ) is ODataPathTemplate template )
            {
                yield return template;
            }
        }
    }
}