namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    sealed class ApiParameterContext
    {
        private ODataPathTemplate? pathTemplate;

        internal ApiParameterContext(
            IModelMetadataProvider metadataProvider,
            ODataRouteBuilderContext routeContext,
            IModelTypeBuilder modelTypeBuilder )
        {
            RouteContext = routeContext;
            MetadataProvider = metadataProvider;
            TypeBuilder = modelTypeBuilder;
        }

        internal ODataRouteBuilderContext RouteContext { get; }

        internal IModelMetadataProvider MetadataProvider { get; }

        internal IList<ApiParameterDescription> Results { get; } = new List<ApiParameterDescription>();

        internal IServiceProvider Services => RouteContext.Services;

        internal IModelTypeBuilder TypeBuilder { get; }

        internal ODataPathTemplate PathTemplate
        {
            get
            {
                if ( pathTemplate != null )
                {
                    return pathTemplate;
                }

                if ( RouteContext.ActionDescriptor.AttributeRouteInfo is ODataAttributeRouteInfo routeInfo )
                {
                    if ( ( pathTemplate = routeInfo.ODataTemplate ) != null )
                    {
                        return pathTemplate;
                    }
                }

                var routeBuilder = new ODataRouteBuilder( RouteContext );
                var path = default( string );

                if ( RouteContext.IsAttributeRouted )
                {
                    path = routeBuilder.BuildPath();
                }
                else
                {
                    using ( new PseudoParameterDescriptionScope( RouteContext ) )
                    {
                        path = routeBuilder.BuildPath();
                    }
                }

                pathTemplate = RouteContext.PathTemplateHandler.ParseTemplate( path, Services );

                return pathTemplate;
            }
        }

        sealed class PseudoParameterDescriptionScope : IDisposable
        {
            readonly ODataRouteBuilderContext context;
            readonly IReadOnlyList<ApiParameterDescription> original;

            internal PseudoParameterDescriptionScope( ODataRouteBuilderContext context )
            {
                this.context = context;

                var parameters = context.ParameterDescriptions;

                original = parameters.ToArray();
                parameters.Clear();

                for ( var i = 0; i < context.ActionDescriptor.Parameters.Count; i++ )
                {
                    var parameter = context.ActionDescriptor.Parameters[i];

                    parameters.Add( new ApiParameterDescription()
                    {
                        Name = parameter.Name,
                        ParameterDescriptor = parameter,
                        Type = parameter.ParameterType,
                    } );
                }
            }

            public void Dispose()
            {
                context.ParameterDescriptions.Clear();

                for ( var i = 0; i < original.Count; i++ )
                {
                    context.ParameterDescriptions.Add( original[i] );
                }
            }
        }
    }
}