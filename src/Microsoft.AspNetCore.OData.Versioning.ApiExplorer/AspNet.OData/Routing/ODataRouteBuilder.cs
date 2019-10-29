namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNet.OData.Routing.ODataRouteConstants;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
    using static System.String;
    using static System.StringComparison;

    partial class ODataRouteBuilder
    {
        internal string BuildPath()
        {
            var segments = new List<string>();
            AppendEntitySetOrOperation( segments );
            return Join( "/", segments );
        }

        void AddOrReplaceNavigationPropertyParameter()
        {
            var parameters = Context.ParameterDescriptions;
            var parameter = default( ApiParameterDescription );

            for ( var i = 0; i < parameters.Count; i++ )
            {
                if ( parameters[i].Name.Equals( NavigationProperty, OrdinalIgnoreCase ) )
                {
                    break;
                }
            }

            if ( parameter == null )
            {
                var type = typeof( string );

                parameter = new ApiParameterDescription()
                {
                    DefaultValue = default( string ),
                    IsRequired = true,
                    ModelMetadata = new ODataQueryOptionModelMetadata( Context.ModelMetadataProvider!, type, description: Empty ),
                    Name = NavigationProperty,
                    ParameterDescriptor = new ParameterDescriptor()
                    {
                        Name = NavigationProperty,
                        ParameterType = type,
                    },
                    Type = type,
                };

                parameters.Add( parameter );
            }

            parameter.Source = Path;
        }

        void AddOrReplaceRefIdQueryParameter()
        {
            var parameters = Context.ParameterDescriptions;
            var parameter = default( ApiParameterDescription );

            for ( var i = 0; i < parameters.Count; i++ )
            {
                if ( parameters[i].Name.Equals( RelatedKey, OrdinalIgnoreCase ) )
                {
                    parameter = parameters[i];
                    break;
                }
            }

            var services = Context.Services;
            var omitPrefix = services.GetRequiredService<ODataUriResolver>().EnableNoDollarQueryOptions;
            var name = omitPrefix ? "id" : "$id";
            var description = Context.Options.RelatedEntityIdParameterDescription;
            var type = typeof( Uri );

            if ( parameter == null )
            {
                parameter = new ApiParameterDescription() { ParameterDescriptor = new ParameterDescriptor() };
                parameters.Add( parameter );
            }

            parameter.IsRequired = true;
            parameter.ModelMetadata = new ODataQueryOptionModelMetadata( Context.ModelMetadataProvider!, type, description );
            parameter.Name = name;
            parameter.ParameterDescriptor.Name = name;
            parameter.ParameterDescriptor.ParameterType = type;
            parameter.Source = Query;
            parameter.Type = type;
        }

        void AddOrReplaceIdBodyParameter()
        {
            var parameters = Context.ParameterDescriptions;
            var parameter = default( ApiParameterDescription );
            var type = typeof( Uri );

            for ( var i = parameters.Count - 1; i >= 0; i-- )
            {
                if ( parameters[i].ParameterDescriptor.ParameterType == type &&
                     parameters[i].Source == Body )
                {
                    parameter = parameters[i];
                    break;
                }
            }

            type = typeof( ODataId );

            if ( parameter == null )
            {
                parameter = new ApiParameterDescription()
                {
                    DefaultValue = default( ODataId ),
                    Name = RelatedKey,
                    ParameterDescriptor = new ParameterDescriptor() { Name = RelatedKey },
                };

                parameters.Add( parameter );
            }

            parameter.IsRequired = true;
            parameter.ModelMetadata = new ODataQueryOptionModelMetadata( Context.ModelMetadataProvider!, type, description: Empty );
            parameter.ParameterDescriptor.ParameterType = type;
            parameter.Source = Body;
            parameter.Type = type;
        }

        static string RemoveRouteConstraints( string routePrefix )
        {
            var parsedTemplate = TemplateParser.Parse( routePrefix );
            var segments = new List<string>( parsedTemplate.Segments.Count );

            for ( var i = 0; i < parsedTemplate.Segments.Count; i++ )
            {
                var currentSegment = Empty;
                var parts = parsedTemplate.Segments[i].Parts;

                for ( var j = 0; j < parts.Count; j++ )
                {
                    var part = parts[j];

                    if ( part.IsLiteral )
                    {
                        currentSegment += part.Text;
                    }
                    else if ( part.IsParameter )
                    {
                        currentSegment += Concat( "{", part.Name, "}" );
                    }
                }

                segments.Add( currentSegment );
            }

            return Join( "/", segments );
        }
    }
}