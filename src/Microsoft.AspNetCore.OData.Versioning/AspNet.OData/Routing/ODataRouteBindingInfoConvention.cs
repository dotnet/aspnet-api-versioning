namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNet.OData.Routing.ODataRouteActionType;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.Linq.Enumerable;
    using static System.StringComparison;

    sealed class ODataRouteBindingInfoConvention : IODataActionDescriptorConvention
    {
        readonly IOptions<ODataApiVersioningOptions> options;

        internal ODataRouteBindingInfoConvention(
            IODataRouteCollectionProvider routeCollectionProvider,
            IModelMetadataProvider modelMetadataProvider,
            IOptions<ODataApiVersioningOptions> options )
        {
            RouteCollectionProvider = routeCollectionProvider;
            ModelMetadataProvider = modelMetadataProvider;
            this.options = options;
        }

        IODataRouteCollectionProvider RouteCollectionProvider { get; }

        IModelMetadataProvider ModelMetadataProvider { get; }

        ODataApiVersioningOptions Options => options.Value;

        ODataAttributeRouteInfoComparer Comparer { get; } = new ODataAttributeRouteInfoComparer();

        public void Apply( ActionDescriptorProviderContext context, ControllerActionDescriptor action )
        {
            var model = action.GetApiVersionModel( Explicit | Implicit );
            var routeInfos = model.IsApiVersionNeutral ?
                             ExpandVersionNeutralActions( action ) :
                             ExpandVersionedActions( action, model );

            foreach ( var routeInfo in routeInfos )
            {
                context.Results.Add( Clone( action, routeInfo ) );
            }
        }

        static ControllerActionDescriptor Clone( ControllerActionDescriptor action, AttributeRouteInfo attributeRouteInfo )
        {
            var clone = new ControllerActionDescriptor()
            {
                ActionConstraints = action.ActionConstraints,
                ActionName = action.ActionName,
                AttributeRouteInfo = attributeRouteInfo,
                BoundProperties = action.BoundProperties,
                ControllerName = action.ControllerName,
                ControllerTypeInfo = action.ControllerTypeInfo,
                DisplayName = action.DisplayName,
                FilterDescriptors = action.FilterDescriptors,
                MethodInfo = action.MethodInfo,
                Parameters = action.Parameters,
                Properties = action.Properties,
                RouteValues = action.RouteValues,
            };

            return clone;
        }

        static void UpdateBindingInfo(
            ControllerActionDescriptor action,
            ODataRouteMapping mapping,
            ICollection<ODataAttributeRouteInfo> routeInfos )
        {
            string template;
            string path;

            switch ( action.ActionName )
            {
                case nameof( MetadataController.GetMetadata ):
                case nameof( VersionedMetadataController.GetOptions ):
                    path = "$metadata";

                    if ( string.IsNullOrEmpty( mapping.RoutePrefix ) )
                    {
                        template = path;
                    }
                    else
                    {
                        template = mapping.RoutePrefix + '/' + path;
                    }

                    break;
                default:
                    path = "/";
                    template = string.IsNullOrEmpty( mapping.RoutePrefix ) ? path : mapping.RoutePrefix;
                    break;
            }

            var handler = mapping.Services.GetRequiredService<IODataPathTemplateHandler>();
            var routeInfo = new ODataAttributeRouteInfo()
            {
                Template = template,
                ODataTemplate = handler.ParseTemplate( path, mapping.Services ),
                RouteName = mapping.RouteName,
                RoutePrefix = mapping.RoutePrefix,
            };

            routeInfos.Add( routeInfo );
        }

        void UpdateBindingInfo(
            ControllerActionDescriptor action,
            ApiVersion apiVersion,
            ODataRouteMapping mapping,
            ICollection<ODataAttributeRouteInfo> routeInfos )
        {
            var routeContext = new ODataRouteBuilderContext( apiVersion, mapping, action, Options );

            if ( routeContext.IsRouteExcluded )
            {
                return;
            }

            var routeBuilder = new ODataRouteBuilder( routeContext );
            var parameterContext = new ActionParameterContext( routeBuilder, routeContext );

            if ( !parameterContext.IsSupported )
            {
                return;
            }

            for ( var i = 0; i < action.Parameters.Count; i++ )
            {
                UpdateBindingInfo( parameterContext, action.Parameters[i] );
            }

            var templates = parameterContext.Templates;

            for ( var i = 0; i < templates.Count; i++ )
            {
                var template = templates[i];
                var routeInfo = new ODataAttributeRouteInfo()
                {
                    Template = template.RouteTemplate,
                    ODataTemplate = template.PathTemplate,
                    RouteName = mapping.RouteName,
                    RoutePrefix = mapping.RoutePrefix,
                };

                routeInfos.Add( routeInfo );
            }

            if ( routeContext.IsOperation )
            {
                EnsureOperationHttpMethod( action, routeContext.Operation! );
            }
        }

        void UpdateBindingInfo( ActionParameterContext context, ParameterDescriptor parameter )
        {
            var parameterType = parameter.ParameterType;
            var bindingInfo = parameter.BindingInfo;

            if ( bindingInfo == null || bindingInfo.BindingSource == null )
            {
                var metadata = ModelMetadataProvider.GetMetadataForType( parameterType );

                if ( bindingInfo == null )
                {
                    parameter.BindingInfo = bindingInfo = new BindingInfo() { BindingSource = metadata.BindingSource };
                }
                else
                {
                    bindingInfo.BindingSource = metadata.BindingSource;
                }
            }

            if ( bindingInfo.BindingSource == Custom )
            {
                if ( parameterType.IsODataQueryOptions() || parameterType.IsODataPath() )
                {
                    bindingInfo.BindingSource = Special;
                }
                else if ( bindingInfo.BinderType.IsODataModelBinder() )
                {
                    bindingInfo.BindingSource = default;
                }
            }

            if ( bindingInfo.BindingSource != null )
            {
                return;
            }

            var key = default( IEdmNamedElement );
            var paramName = parameter.Name;
            var source = Query;

            switch ( context.RouteContext.ActionType )
            {
                case EntitySet:
                    var keys = context.RouteContext.EntitySet.EntityType().Key();

                    key = keys.FirstOrDefault( k => k.Name.Equals( paramName, OrdinalIgnoreCase ) );

                    if ( key == null )
                    {
                        var template = context.Templates[0].PathTemplate;
                        var segments = template.Segments.OfType<KeySegmentTemplate>();

                        if ( segments.SelectMany( s => s.ParameterMappings.Values ).Any( name => name.Equals( paramName, OrdinalIgnoreCase ) ) )
                        {
                            source = Path;
                        }
                    }
                    else
                    {
                        source = Path;
                    }

                    break;
                case BoundOperation:
                case UnboundOperation:
                    var operation = context.RouteContext.Operation;

                    if ( operation == null )
                    {
                        break;
                    }

                    key = operation.Parameters.FirstOrDefault( p => p.Name.Equals( paramName, OrdinalIgnoreCase ) );

                    if ( key == null )
                    {
                        if ( operation.IsBound )
                        {
                            goto case EntitySet;
                        }
                    }
                    else
                    {
                        source = Path;
                    }

                    break;
            }

            bindingInfo.BindingSource = source;
        }

        IEnumerable<ODataAttributeRouteInfo> ExpandVersionedActions( ControllerActionDescriptor action, ApiVersionModel model )
        {
            var mappings = RouteCollectionProvider.Items;
            var routeInfos = new HashSet<ODataAttributeRouteInfo>( Comparer );
            var declaredVersions = model.DeclaredApiVersions;
            var metadata = action.ControllerTypeInfo.IsMetadataController();

            for ( var i = 0; i < declaredVersions.Count; i++ )
            {
                for ( var j = 0; j < mappings.Count; j++ )
                {
                    var mapping = mappings[j];
                    var selector = mapping.ModelSelector;

                    if ( !selector.Contains( declaredVersions[i] ) )
                    {
                        continue;
                    }

                    if ( metadata )
                    {
                        UpdateBindingInfo( action, mapping, routeInfos );
                    }
                    else
                    {
                        var mappedVersions = selector.ApiVersions;

                        for ( var k = 0; k < mappedVersions.Count; k++ )
                        {
                            UpdateBindingInfo( action, mappedVersions[k], mapping, routeInfos );
                        }
                    }
                }
            }

            return routeInfos;
        }

        IEnumerable<ODataAttributeRouteInfo> ExpandVersionNeutralActions( ControllerActionDescriptor action )
        {
            var mappings = RouteCollectionProvider.Items;
            var routeInfos = new HashSet<ODataAttributeRouteInfo>( Comparer );
            var visited = new HashSet<ApiVersion>();

            for ( var i = 0; i < mappings.Count; i++ )
            {
                var mapping = mappings[i];
                var mappedVersions = mapping.ModelSelector.ApiVersions;

                for ( var j = 0; j < mappedVersions.Count; j++ )
                {
                    var apiVersion = mappedVersions[j];

                    if ( visited.Add( apiVersion ) )
                    {
                        UpdateBindingInfo( action, apiVersion, mapping, routeInfos );
                    }
                }
            }

            return routeInfos;
        }

        static void EnsureOperationHttpMethod( ControllerActionDescriptor actionDescriptor, IEdmOperation operation )
        {
            static void LimitByODataSpec( ControllerActionDescriptor action, string httpMethod )
            {
                if ( action.ActionConstraints is null )
                {
                    action.ActionConstraints = new List<IActionConstraintMetadata>();
                }
                else
                {
                    for ( var i = action.ActionConstraints.Count - 1; i >= 0; i-- )
                    {
                        if ( action.ActionConstraints[i] is HttpMethodActionConstraint )
                        {
                            action.ActionConstraints.RemoveAt( i );
                        }
                    }
                }

                action.ActionConstraints.Add( new HttpMethodActionConstraint( new[] { httpMethod } ) );
            }

            if ( operation.IsFunction() )
            {
                LimitByODataSpec( actionDescriptor, "GET" );
            }
            else if ( operation.IsAction() )
            {
                LimitByODataSpec( actionDescriptor, "POST" );
            }
        }

        sealed class ODataAttributeRouteInfoComparer : IEqualityComparer<ODataAttributeRouteInfo>
        {
            public bool Equals( ODataAttributeRouteInfo? x, ODataAttributeRouteInfo? y )
            {
                if ( x == null )
                {
                    return y == null;
                }
                else if ( y == null )
                {
                    return false;
                }

                var comparer = StringComparer.OrdinalIgnoreCase;

                return comparer.Equals( x.Template, y.Template ) &&
                       comparer.Equals( x.RouteName, y.RouteName );
            }

            public int GetHashCode( ODataAttributeRouteInfo obj ) =>
                obj is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode( obj.Template );
        }
    }
}