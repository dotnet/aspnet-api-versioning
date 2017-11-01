namespace Microsoft.Web.Http.Description
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.Dispatcher;
    using System.Web.OData.Routing;
    using static System.Linq.Enumerable;

    sealed class ODataRouteBuilderContext
    {
        readonly IServiceProvider serviceProvider;
        readonly Lazy<IEdmEntityType> entityType;
        readonly Lazy<IEnumerable<IEdmStructuralProperty>> entityKeys;

        internal ODataRouteBuilderContext(
            HttpConfiguration configuration,
            string routeTemplate,
            ODataRoute route,
            HttpActionDescriptor actionDescriptor,
            IReadOnlyList<ApiParameterDescription> parameterDescriptions )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( !string.IsNullOrEmpty( routeTemplate ) );
            Contract.Requires( route != null );
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( parameterDescriptions != null );

            serviceProvider = configuration.GetODataRootContainer( route );
            AssembliesResolver = configuration.Services.GetAssembliesResolver();
            EdmModel = serviceProvider.GetRequiredService<IEdmModel>();
            RouteTemplate = routeTemplate;
            Route = route;
            ActionDescriptor = actionDescriptor;
            ParameterDescriptions = parameterDescriptions;
            entityType = new Lazy<IEdmEntityType>( ResolveEntityType );
            entityKeys = new Lazy<IEnumerable<IEdmStructuralProperty>>( () => EntityType?.Key() ?? Empty<IEdmStructuralProperty>() );
        }

        internal IAssembliesResolver AssembliesResolver { get; }

        internal IEdmModel EdmModel { get; }

        internal string RouteTemplate { get; }

        internal ODataRoute Route { get; }

        internal HttpActionDescriptor ActionDescriptor { get; }

        internal IReadOnlyList<ApiParameterDescription> ParameterDescriptions { get; }

        internal IEdmEntityType EntityType => entityType.Value;

        internal IEnumerable<IEdmStructuralProperty> EntityKeys => entityKeys.Value;

        internal bool AllowUnqualifiedEnum => serviceProvider.GetRequiredService<ODataUriResolver>() is StringAsEnumResolver;

        IEdmEntityType ResolveEntityType()
        {
            var entitySetName = ActionDescriptor.ControllerDescriptor.ControllerName;
            return EdmModel.EntityContainer?.FindEntitySet( entitySetName )?.EntityType();
        }
    }
}