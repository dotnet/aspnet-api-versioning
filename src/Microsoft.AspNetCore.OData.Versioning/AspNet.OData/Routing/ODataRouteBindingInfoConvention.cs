namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using static Microsoft.AspNet.OData.Routing.ODataRouteActionType;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
    using static System.Linq.Enumerable;
    using static System.StringComparison;

    sealed class ODataRouteBindingInfoConvention : IODataActionDescriptorConvention
    {
        readonly IEnumerable<Assembly> assemblies;

        internal ODataRouteBindingInfoConvention( IODataRouteCollectionProvider routeCollectionProvider, ApplicationPartManager partManager )
        {
            Contract.Requires( routeCollectionProvider != null );
            Contract.Requires( partManager != null );

            RouteCollectionProvider = routeCollectionProvider;
            assemblies = partManager.ApplicationParts.OfType<AssemblyPart>().Select( p => p.Assembly ).ToArray();
        }

        IODataRouteCollectionProvider RouteCollectionProvider { get; }

        public void Apply( ActionDescriptorProviderContext context, ControllerActionDescriptor action )
        {
            Contract.Requires( context != null );
            Contract.Requires( action != null );

            var model = action.GetApiVersionModel();
            var mappings = RouteCollectionProvider.Items;
            var routeInfos = new HashSet<ODataAttributeRouteInfo>( new ODataAttributeRouteInfoComparer() );

            UpdateControllerName( action );

            if ( model.IsApiVersionNeutral )
            {
                if ( mappings.Count == 0 )
                {
                    return;
                }

                // note: any mapping will do for a version-neutral action; just take the first one
                var mapping = mappings[0];

                UpdateBindingInfo( action, mapping, routeInfos );
            }
            else
            {
                foreach ( var apiVersion in model.DeclaredApiVersions )
                {
                    if ( !mappings.TryGetValue( apiVersion, out var mappingsPerApiVersion ) )
                    {
                        continue;
                    }

                    foreach ( var mapping in mappingsPerApiVersion )
                    {
                        UpdateBindingInfo( action, mapping, routeInfos );
                    }
                }
            }

            if ( routeInfos.Count == 0 )
            {
                return;
            }

            using ( var iterator = routeInfos.GetEnumerator() )
            {
                iterator.MoveNext();
                action.AttributeRouteInfo = iterator.Current;

                while ( iterator.MoveNext() )
                {
                    context.Results.Add( Clone( action, iterator.Current ) );
                }
            }
        }

        void UpdateBindingInfo( ControllerActionDescriptor action, ODataRouteMapping mapping, ICollection<ODataAttributeRouteInfo> routeInfos )
        {
            Contract.Requires( action != null );
            Contract.Requires( mapping != null );
            Contract.Requires( routeInfos != null );

            var routeContext = new ODataRouteBuilderContext( assemblies, mapping, action );
            var routeBuilder = new ODataRouteBuilder( routeContext );
            var parameterContext = new ActionParameterContext( routeBuilder, routeContext );

            foreach ( var parameter in action.Parameters )
            {
                UpdateBindingInfo( parameterContext, parameter );
            }

            var routeInfo = new ODataAttributeRouteInfo()
            {
                Template = routeBuilder.BuildPath( includePrefix: true ),
                ODataTemplate = parameterContext.PathTemplate,
            };

            routeInfos.Add( routeInfo );
        }

        static void UpdateBindingInfo( ActionParameterContext context, ParameterDescriptor parameter )
        {
            Contract.Requires( context != null );
            Contract.Requires( parameter != null );

            var bindingInfo = parameter.BindingInfo;

            if ( bindingInfo != null && bindingInfo.BindingSource != Custom )
            {
                return;
            }

            bindingInfo = parameter.BindingInfo ?? new BindingInfo();

            var paramType = parameter.ParameterType;

            if ( paramType.IsODataQueryOptions() || paramType.IsODataPath() )
            {
                bindingInfo.BindingSource = ModelBinding;
                parameter.BindingInfo = bindingInfo;
                return;
            }
            else if ( paramType.IsDelta() )
            {
                bindingInfo.BindingSource = Body;
                parameter.BindingInfo = bindingInfo;
                return;
            }

            var key = default( IEdmNamedElement );
            var paramName = parameter.Name;
            var source = Query;

            switch ( context.RouteContext.ActionType )
            {
                case EntitySet:

                    var keys = context.RouteContext.EntitySet.EntityType().Key().ToArray();

                    key = keys.FirstOrDefault( k => k.Name.Equals( paramName, OrdinalIgnoreCase ) );

                    if ( key == null )
                    {
                        var template = context.PathTemplate;

                        if ( template != null )
                        {
                            var segments = template.Segments.OfType<KeySegmentTemplate>();

                            if ( segments.SelectMany( s => s.ParameterMappings.Values ).Any( name => name.Equals( paramName, OrdinalIgnoreCase ) ) )
                            {
                                source = Path;
                            }
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

                    if ( paramType.IsODataActionParameters() )
                    {
                        source = Body;
                    }
                    else
                    {
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
                    }

                    break;
            }

            bindingInfo.BindingSource = source;
            parameter.BindingInfo = bindingInfo;
        }

        static ControllerActionDescriptor Clone( ControllerActionDescriptor action, AttributeRouteInfo attributeRouteInfo )
        {
            Contract.Requires( action != null );
            Contract.Requires( attributeRouteInfo != null );
            Contract.Ensures( Contract.Result<ControllerActionDescriptor>() != null );

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

        static void UpdateControllerName( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );

            if ( !action.RouteValues.TryGetValue( "controller", out var key ) )
            {
                key = action.ControllerName;
            }

            action.ControllerName = TrimTrailingNumbers( key );
        }

        static string TrimTrailingNumbers( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return name;
            }

            var last = name.Length - 1;

            for ( var i = last; i >= 0; i-- )
            {
                if ( !char.IsNumber( name[i] ) )
                {
                    if ( i < last )
                    {
                        return name.Substring( 0, i + 1 );
                    }

                    return name;
                }
            }

            return name;
        }

        sealed class ODataAttributeRouteInfoComparer : IEqualityComparer<ODataAttributeRouteInfo>
        {
            public bool Equals( ODataAttributeRouteInfo x, ODataAttributeRouteInfo y )
            {
                if ( x == null )
                {
                    return y == null;
                }
                else if ( y == null )
                {
                    return false;
                }

                return StringComparer.OrdinalIgnoreCase.Equals( x.Template, y.Template );
            }

            public int GetHashCode( ODataAttributeRouteInfo obj ) => obj is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode( obj.Template );
        }
    }
}