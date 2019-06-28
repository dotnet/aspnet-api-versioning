namespace Microsoft.AspNet.OData.Routing
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
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
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Description;
    using System.Web.Http.Dispatcher;
    using ControllerActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
#endif
    using static ODataRouteTemplateGenerationKind;

    sealed partial class ODataRouteBuilderContext
    {
        readonly IServiceProvider serviceProvider;
        readonly ODataRouteAttribute routeAttribute;

        internal ApiVersion ApiVersion { get; }

#if API_EXPLORER
        internal ODataApiExplorerOptions Options { get; }

        internal IList<ApiParameterDescription> ParameterDescriptions { get; }

        internal ODataRouteTemplateGenerationKind RouteTemplateGeneration { get; } = Client;
#else
        internal IList<ParameterDescriptor> ParameterDescriptions => ActionDescriptor.Parameters;

        internal ODataRouteTemplateGenerationKind RouteTemplateGeneration { get; } = Server;
#endif

        internal IEdmModel EdmModel { get; }

        internal string RouteTemplate { get; }

        internal ODataRoute Route { get; }

        internal ControllerActionDescriptor ActionDescriptor { get; }

        internal IEdmEntitySet EntitySet { get; }

        internal IEdmOperation Operation { get; }

        internal ODataRouteActionType ActionType { get; }

        internal ODataUrlKeyDelimiter UrlKeyDelimiter { get; }

        internal bool IsRouteExcluded { get; }

        internal bool IsAttributeRouted => routeAttribute != null;

        internal bool IsOperation => Operation != null;

        internal bool IsBound => IsOperation && EntitySet != null;

        internal bool AllowUnqualifiedEnum => serviceProvider.GetRequiredService<ODataUriResolver>() is StringAsEnumResolver;

        internal static ODataRouteActionType GetActionType( IEdmEntitySet entitySet, IEdmOperation operation )
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
                    return ODataRouteActionType.EntitySet;
                }
                else if ( operation.IsBound )
                {
                    return ODataRouteActionType.BoundOperation;
                }
            }

            return ODataRouteActionType.Unknown;
        }
    }
}