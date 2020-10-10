﻿namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.UriParser;
    using System;
    using System.Web.Http.Description;
    using static Microsoft.AspNet.OData.Routing.ODataRouteConstants;
    using static System.StringComparison;
    using static System.Web.Http.Description.ApiParameterSource;

    partial class ODataRouteBuilder
    {
        internal void AddOrReplaceRefIdQueryParameter()
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

            var omitPrefix = Context.Services.GetRequiredService<ODataUriResolver>().EnableNoDollarQueryOptions;
            var name = omitPrefix ? "id" : "$id";

            if ( parameter == null )
            {
                parameter = new ApiParameterDescription();
                parameters.Add( parameter );
            }

            var descriptor = new ODataQueryOptionParameterDescriptor( name, typeof( Uri ), default( Uri ), optional: false )
            {
                ActionDescriptor = Context.ActionDescriptor,
                Configuration = Context.ActionDescriptor.Configuration,
            };

            parameter.Name = name;
            parameter.Source = FromUri;
            parameter.Documentation = Context.Options.RelatedEntityIdParameterDescription;
            parameter.ParameterDescriptor = descriptor;

            if ( omitPrefix )
            {
                descriptor.SetPrefix( string.Empty );
            }
        }

        void AddOrReplaceIdBodyParameter()
        {
            var parameters = Context.ParameterDescriptions;
            var parameter = default( ApiParameterDescription );
            var type = typeof( Uri );

            for ( var i = parameters.Count - 1; i >= 0; i-- )
            {
                parameter = parameters[i];

                if ( parameter.Source == FromBody &&
                     parameter.ParameterDescriptor?.ParameterType == type )
                {
                    break;
                }

                parameter = default;
            }

            if ( parameter == null )
            {
                parameter = new ApiParameterDescription() { Name = RelatedKey, Source = FromBody };
                parameters.Add( parameter );
            }

            parameter.ParameterDescriptor = new ODataParameterDescriptor( RelatedKey, typeof( ODataId ) )
            {
                ActionDescriptor = Context.ActionDescriptor,
                Configuration = Context.ActionDescriptor.Configuration,
            };
        }

        static string RemoveRouteConstraints( string routePrefix ) => routePrefix;
    }
}