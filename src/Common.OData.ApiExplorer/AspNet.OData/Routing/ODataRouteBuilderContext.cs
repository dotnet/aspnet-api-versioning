namespace Microsoft.AspNet.OData.Routing
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
#if WEBAPI
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
#if WEBAPI
    using System.Web.Http.Description;
    using System.Web.Http.Dispatcher;
    using ControllerActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
#endif
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static ODataRouteTemplateGenerationKind;
    using static System.StringComparison;

    sealed partial class ODataRouteBuilderContext
    {
        readonly ODataRouteAttribute? routeAttribute;

        internal IServiceProvider Services { get; }

        internal ApiVersion ApiVersion { get; }

#if API_EXPLORER
        internal ODataApiExplorerOptions Options { get; }

        internal IList<ApiParameterDescription> ParameterDescriptions { get; }

        internal ODataRouteTemplateGenerationKind RouteTemplateGeneration { get; } = Client;
#else
        internal ODataApiVersioningOptions Options { get; }

        internal IList<ParameterDescriptor> ParameterDescriptions => ActionDescriptor.Parameters;

        internal ODataRouteTemplateGenerationKind RouteTemplateGeneration { get; } = Server;
#endif

        internal IEdmModel EdmModel { get; }

        internal string? RouteTemplate { get; }

        internal string? RoutePrefix { get; }

        internal ControllerActionDescriptor ActionDescriptor { get; }

        internal IEdmEntitySet? EntitySet { get; }

        internal IEdmOperation? Operation { get; }

        internal ODataRouteActionType ActionType { get; }

        internal ODataUrlKeyDelimiter UrlKeyDelimiter { get; }

        internal bool IsRouteExcluded { get; }

        internal bool IsAttributeRouted => routeAttribute != null;

        internal bool IsOperation => Operation != null;

        internal bool IsBound => IsOperation && EntitySet != null;

        internal bool AllowUnqualifiedEnum => Services.GetRequiredService<ODataUriResolver>() is StringAsEnumResolver;

        internal
#if !WEBAPI
        static
#endif
        ODataRouteActionType GetActionType( IEdmEntitySet? entitySet, IEdmOperation? operation, ControllerActionDescriptor action )
        {
            if ( entitySet == null )
            {
                if ( operation == null )
                {
                    return ODataRouteActionType.Unknown;
                }
                else if ( !operation.IsBound )
                {
                    return ODataRouteActionType.UnboundOperation;
                }
            }
            else
            {
                if ( operation == null )
                {
                    if ( IsActionOrFunction( entitySet, action.ActionName, GetHttpMethods( action ) ) )
                    {
                        return ODataRouteActionType.Unknown;
                    }
                    else
                    {
                        return ODataRouteActionType.EntitySet;
                    }
                }
                else if ( operation.IsBound )
                {
                    return ODataRouteActionType.BoundOperation;
                }
            }

            return ODataRouteActionType.Unknown;
        }

        // Slash became the default 4/18/2018
        // REF: https://github.com/OData/WebApi/pull/1393
        static ODataUrlKeyDelimiter UrlKeyDelimiterOrDefault( ODataUrlKeyDelimiter? urlKeyDelimiter ) => urlKeyDelimiter ?? Slash;

        // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/ActionRoutingConvention.cs
        // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/FunctionRoutingConvention.cs
        // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/EntitySetRoutingConvention.cs
        // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/EntityRoutingConvention.cs
        static bool IsActionOrFunction( IEdmEntitySet? entitySet, string actionName, IEnumerable<string> methods )
        {
            using var iterator = methods.GetEnumerator();

            if ( !iterator.MoveNext() )
            {
                return false;
            }

            var method = iterator.Current;

            if ( iterator.MoveNext() )
            {
                return false;
            }

            const string ActionMethod = "Post";
            const string FunctionMethod = "Get";

            if ( ActionMethod.Equals( method, OrdinalIgnoreCase ) && actionName != ActionMethod )
            {
                if ( entitySet == null )
                {
                    return true;
                }

                return actionName != ( ActionMethod + entitySet.Name ) &&
                       actionName != ( ActionMethod + entitySet.EntityType().Name ) &&
                      !actionName.StartsWith( "CreateRef", Ordinal );
            }
            else if ( FunctionMethod.Equals( method, OrdinalIgnoreCase ) && actionName != FunctionMethod )
            {
                if ( entitySet == null )
                {
                    // TODO: could be a singleton here
                    return true;
                }

                if ( actionName == ( ActionMethod + entitySet.Name ) ||
                     actionName.StartsWith( "GetRef", Ordinal ) )
                {
                    return false;
                }

                var entity = entitySet.EntityType();

                if ( actionName == ( ActionMethod + entity.Name ) )
                {
                    return false;
                }

                foreach ( var property in entity.NavigationProperties() )
                {
                    if ( actionName.StartsWith( FunctionMethod + property.Name, OrdinalIgnoreCase ) )
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        IEdmOperation? ResolveOperation( IEdmEntityContainer container, string name )
        {
            var import = container.FindOperationImports( name ).SingleOrDefault();

            if ( import != null )
            {
                return import.Operation;
            }

            var qualifiedName = container.Namespace + "." + name;

            if ( EntitySet != null )
            {
                var operation = EdmModel.FindBoundOperations( qualifiedName, EntitySet.EntityType() ).SingleOrDefault();

                if ( operation != null )
                {
                    return operation;
                }
            }

            return EdmModel.FindDeclaredOperations( qualifiedName ).SingleOrDefault();
        }

        sealed class FixedEdmModelServiceProviderDecorator : IServiceProvider
        {
            readonly IServiceProvider decorated;
            readonly IEdmModel edmModel;

            internal FixedEdmModelServiceProviderDecorator( IServiceProvider decorated, IEdmModel edmModel )
            {
                this.decorated = decorated;
                this.edmModel = edmModel;
            }

            public object GetService( Type serviceType ) =>
                serviceType == typeof( IEdmModel ) ? edmModel : decorated.GetService( serviceType );
        }
    }
}