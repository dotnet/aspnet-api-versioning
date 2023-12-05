// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using Asp.Versioning.Controllers;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using static Asp.Versioning.ApiVersionMapping;
using static System.StringComparer;

/// <summary>
/// Represents the logic for selecting a versioned controller.
/// </summary>
public class ApiVersionControllerSelector : IHttpControllerSelector
{
    private readonly HttpConfiguration configuration;
    private readonly ApiVersioningOptions options;
    private readonly HttpControllerTypeCache controllerTypeCache;
    private readonly Lazy<IDictionary<string, HttpControllerDescriptor>> controllerInfoCache;
    private bool initializing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionControllerSelector"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> to initialize
    /// the controller selector with.</param>
    /// <param name="options">The <see cref="ApiVersioningOptions">service versioning options</see>
    /// associated with the controller selector.</param>
    public ApiVersionControllerSelector( HttpConfiguration configuration, ApiVersioningOptions options )
    {
        this.configuration = configuration;
        this.options = options;
        controllerInfoCache = new( InitializeControllerInfoCache );
        controllerTypeCache = new( this.configuration );
    }

    /// <summary>
    /// Creates and returns a controller descriptor mapping.
    /// </summary>
    /// <returns>A <see cref="IDictionary{TKey,TValue}">collection</see> of route-to-controller mapping.</returns>
    public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
    {
        if ( !controllerInfoCache.IsValueCreated )
        {
            if ( initializing )
            {
                throw new InvalidOperationException( SR.ControllerSelectorMappingCycle );
            }

            initializing = true;
        }

        var mapping = controllerInfoCache.Value;

        initializing = false;

        return mapping;
    }

    /// <summary>
    /// Selects and returns the controller descriptor to invoke given the provided request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get a controller descriptor for.</param>
    /// <returns>The <see cref="HttpControllerDescriptor">controller descriptor</see> that matches the specified <paramref name="request"/>.</returns>
    public virtual HttpControllerDescriptor? SelectController( HttpRequestMessage request )
    {
        var context = NewSelectionContext( request );
        var conventionRouteSelector = new ConventionRouteControllerSelector( controllerTypeCache );
        HttpResponseExceptionFactory exceptionFactory;
        ControllerSelectionResult conventionRouteResult;

        if ( context.RouteData == null )
        {
            conventionRouteResult = conventionRouteSelector.SelectController( context );

            if ( conventionRouteResult.Succeeded )
            {
                EnsureUrlHelper( request );
                return request.ApiVersionProperties().SelectedController = conventionRouteResult.Controller;
            }

            exceptionFactory = new( request, context );
            throw exceptionFactory.NewUnmatchedException( conventionRouteResult );
        }

        var directRouteSelector = new DirectRouteControllerSelector();
        var directRouteResult = directRouteSelector.SelectController( context );

        if ( directRouteResult.Succeeded )
        {
            EnsureUrlHelper( request );
            return request.ApiVersionProperties().SelectedController = directRouteResult.Controller;
        }

        conventionRouteResult = conventionRouteSelector.SelectController( context );

        if ( conventionRouteResult.Succeeded )
        {
            EnsureUrlHelper( request );
            return request.ApiVersionProperties().SelectedController = conventionRouteResult.Controller;
        }

        exceptionFactory = new( request, context );
        throw exceptionFactory.NewUnmatchedException( conventionRouteResult, directRouteResult );
    }

    /// <summary>
    /// Gets the name of the controller for the specified request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage">request</see> to the controller name for.</param>
    /// <returns>The name of the controller for the specified <paramref name="request"/>.</returns>
    public virtual string? GetControllerName( HttpRequestMessage request )
    {
        var routeData = request.GetRouteData();

        if ( routeData == null )
        {
            return null;
        }

        if ( routeData.Values.TryGetValue( RouteDataTokenKeys.Controller, out string? controller ) )
        {
            return controller;
        }

        var routes = configuration.Routes;
        var context = request.GetRequestContext();
        var virtualPathRoot = routes.VirtualPathRoot;

        if ( context != null )
        {
            virtualPathRoot = context.VirtualPathRoot ?? string.Empty;
        }

        // HACK: do NOT use a normal 'for' loop here because the IIS implementation does not support indexing
        foreach ( var route in routes )
        {
            var otherRouteData = route.GetRouteData( virtualPathRoot, request );

            if ( otherRouteData != null &&
                !routeData.Equals( otherRouteData ) &&
                 otherRouteData.Values.TryGetValue( RouteDataTokenKeys.Controller, out controller ) )
            {
                break;
            }
        }

        return controller;
    }

    private IDictionary<string, HttpControllerDescriptor> InitializeControllerInfoCache()
    {
        var implicitVersionModel = new ApiVersionModel( options.DefaultApiVersion );
        var conventions = options.Conventions;
        var actionSelector = configuration.Services.GetActionSelector();
        var mapping = new ConcurrentDictionary<string, HttpControllerDescriptor>( OrdinalIgnoreCase );

        foreach ( var pair in controllerTypeCache.Cache )
        {
            var groupings = pair.Value;
            var count = groupings.Count;

            if ( count == 0 )
            {
                continue;
            }

            var key = pair.Key;
            var descriptors = default( List<HttpControllerDescriptor> );

            foreach ( var grouping in groupings )
            {
                foreach ( var type in grouping )
                {
                    var descriptor = new HttpControllerDescriptor( configuration, key, type );

                    if ( !conventions.ApplyTo( descriptor ) )
                    {
                        ApplyAttributeOrImplicitConventions( descriptor, actionSelector, implicitVersionModel );
                    }

                    descriptors ??= new( capacity: count );
                    descriptors.Add( descriptor );
                }
            }

            if ( descriptors == null )
            {
                continue;
            }

            var innerDescriptors = ApplyCollatedModels( configuration, descriptors, actionSelector );
            var descriptorGroup = new HttpControllerDescriptorGroup( configuration, key, innerDescriptors );

            mapping.TryAdd( key, descriptorGroup );
        }

        return new ReadOnlyDictionary<string, HttpControllerDescriptor>( mapping );
    }

    private static bool IsDecoratedWithAttributes( HttpControllerDescriptor controller )
    {
        return controller.GetCustomAttributes<IApiVersionProvider>().Count > 0 ||
               controller.GetCustomAttributes<IApiVersionNeutral>().Count > 0;
    }

    private static void ApplyImplicitConventions( HttpControllerDescriptor controller, IHttpActionSelector actionSelector, ApiVersionModel implicitVersionModel )
    {
        controller.SetApiVersionModel( implicitVersionModel );

        var mapping = actionSelector.GetActionMapping( controller );

        if ( mapping.Count == 0 )
        {
            return;
        }

        var actions = mapping.SelectMany( g => g );
        var namingConvention = controller.Configuration.GetControllerNameConvention();
        var name = namingConvention.GroupName( controller.ControllerName );
        var metadata = new ApiVersionMetadata( implicitVersionModel, implicitVersionModel, name );

        foreach ( var action in actions )
        {
            action.SetApiVersionMetadata( metadata );
        }
    }

    private static void ApplyAttributeOrImplicitConventions( HttpControllerDescriptor controller, IHttpActionSelector actionSelector, ApiVersionModel implicitVersionModel )
    {
        if ( IsDecoratedWithAttributes( controller ) )
        {
            var conventions = new ControllerApiVersionConventionBuilder( controller.ControllerType );
            conventions.ApplyTo( controller );
        }
        else
        {
            ApplyImplicitConventions( controller, actionSelector, implicitVersionModel );
        }
    }

    private static HttpControllerDescriptor[] ApplyCollatedModels(
        HttpConfiguration configuration,
        List<HttpControllerDescriptor> controllers,
        IHttpActionSelector actionSelector )
    {
        var controllerModels = new List<ApiVersionModel>( controllers.Count );
        var actionModels = new List<ApiVersionModel>( controllers.Count );
        var visitedControllers = new List<Tuple<HttpControllerDescriptor, ApiVersionModel>>( controllers.Count );
        var visitedActions = new List<Tuple<HttpControllerDescriptor, HttpActionDescriptor, ApiVersionModel>>( controllers.Count );

        CollateControllerVersions( controllers, actionSelector, controllerModels, actionModels, visitedControllers, visitedActions );
        CollateControllerModels( controllerModels, visitedControllers, CollateActionModels( actionModels, visitedActions ) );
        ApplyCollatedModelsToActions( configuration, visitedActions );

        return [.. controllers];
    }

    private static void CollateControllerVersions(
        List<HttpControllerDescriptor> controllers,
        IHttpActionSelector actionSelector,
        List<ApiVersionModel> controllerModels,
        List<ApiVersionModel> actionModels,
        List<Tuple<HttpControllerDescriptor, ApiVersionModel>> visitedControllers,
        List<Tuple<HttpControllerDescriptor, HttpActionDescriptor, ApiVersionModel>> visitedActions )
    {
        for ( var i = 0; i < controllers.Count; i++ )
        {
            var controller = controllers[i];
            var model = controller.GetApiVersionModel();

            if ( model.IsApiVersionNeutral )
            {
                continue;
            }

            controllerModels.Add( model );
            visitedControllers.Add( Tuple.Create( controller, model ) );

            CollateActionVersions( actionSelector, actionModels, visitedActions, controller );
        }
    }

    private static void CollateActionVersions(
        IHttpActionSelector actionSelector,
        List<ApiVersionModel> actionModels,
        List<Tuple<HttpControllerDescriptor, HttpActionDescriptor, ApiVersionModel>> visitedActions,
        HttpControllerDescriptor controller )
    {
        var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

        foreach ( var action in actions )
        {
            var metadata = action.GetApiVersionMetadata();

            if ( metadata.IsApiVersionNeutral )
            {
                continue;
            }

            var model = metadata.Map( Explicit );
            actionModels.Add( model );
            visitedActions.Add( Tuple.Create( controller, action, model ) );
        }
    }

    private static ApiVersionModel CollateActionModels(
        List<ApiVersionModel> actionModels,
        List<Tuple<HttpControllerDescriptor, HttpActionDescriptor, ApiVersionModel>> visitedActions )
    {
        var collatedModel = actionModels.Aggregate();

        for ( var i = 0; i < visitedActions.Count; i++ )
        {
            var (controller, action, model) = visitedActions[i];
            visitedActions[i] = Tuple.Create( controller, action, model.Aggregate( collatedModel ) );
        }

        return collatedModel;
    }

    private static void CollateControllerModels(
        List<ApiVersionModel> controllerModels,
        List<Tuple<HttpControllerDescriptor, ApiVersionModel>> visitedControllers,
        ApiVersionModel collatedModel )
    {
        // note: allows controllers to report versions in 400s even when an action is unmatched
        controllerModels.Add( collatedModel );
        collatedModel = controllerModels.Aggregate();

        for ( var i = 0; i < visitedControllers.Count; i++ )
        {
            var (controller, model) = visitedControllers[i];
            controller.SetApiVersionModel( model.Aggregate( collatedModel ) );
        }
    }

    private static void ApplyCollatedModelsToActions(
        HttpConfiguration configuration,
        List<Tuple<HttpControllerDescriptor, HttpActionDescriptor, ApiVersionModel>> visitedActions )
    {
        var namingConvention = configuration.GetControllerNameConvention();

        for ( var i = 0; i < visitedActions.Count; i++ )
        {
            var (controller, action, endpointModel) = visitedActions[i];
            var apiModel = controller.GetApiVersionModel();
            var name = namingConvention.GroupName( controller.ControllerName );
            action.SetApiVersionMetadata( new ApiVersionMetadata( apiModel, endpointModel, name ) );
        }
    }

    private static void EnsureUrlHelper( HttpRequestMessage request )
    {
        var context = request.GetRequestContext();

        if ( context == null || context.Url is ApiVersionUrlHelper )
        {
            return;
        }

        var options = request.GetApiVersioningOptions();

        if ( options.ApiVersionReader.VersionsByUrl() )
        {
            context.Url = new ApiVersionUrlHelper( context.Url );
        }
    }

    private ControllerSelectionContext NewSelectionContext( HttpRequestMessage request )
    {
        var properties = request.ApiVersionProperties();
        var context = new ControllerSelectionContext( request, GetControllerName, controllerInfoCache );
        HttpResponseExceptionFactory factory;
        HttpResponseMessage response;

        switch ( properties.RawRequestedApiVersions.Count )
        {
            case 0:
                if ( options.AssumeDefaultVersionWhenUnspecified )
                {
                    properties.RequestedApiVersion = options.ApiVersionSelector.SelectVersion( request, context.AllVersions );
                }

                return context;
            case 1:
                if ( properties.RequestedApiVersion is not null )
                {
                    return context;
                }

                factory = new HttpResponseExceptionFactory( request, context );
                response = factory.CreateBadRequestForInvalidApiVersion();
                break;
            default:
                factory = new HttpResponseExceptionFactory( request, context );
                response = factory.CreateBadRequestForAmbiguousApiVersion( properties.RawRequestedApiVersions );
                break;
        }

        throw new HttpResponseException( response );
    }
}