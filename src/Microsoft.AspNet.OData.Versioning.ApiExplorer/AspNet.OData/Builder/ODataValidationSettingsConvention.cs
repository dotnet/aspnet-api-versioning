namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static System.Web.Http.Description.ApiParameterSource;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ODataValidationSettingsConvention
    {
        /// <inheritdoc />
        public virtual void ApplyTo( ApiDescription apiDescription )
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            if ( !IsSupported( apiDescription ) )
            {
                return;
            }

            var context = new ODataQueryOptionDescriptionContext( ValidationSettings );
            var model = apiDescription.EdmModel();
            var queryOptions = GetQueryOptions( apiDescription.ActionDescriptor.Configuration.GetDefaultQuerySettings(), context );
            var singleResult = IsSingleResult( apiDescription, out var resultType );
            var visitor = new ODataAttributeVisitor( context, model, queryOptions, resultType, singleResult );

            visitor.Visit( apiDescription );

            var options = visitor.AllowedQueryOptions;
            var parameterDescriptions = apiDescription.ParameterDescriptions;

            if ( options.HasFlag( Select ) )
            {
                parameterDescriptions.Add( SetAction( NewSelectParameter( context ), apiDescription ) );
            }

            if ( options.HasFlag( Expand ) )
            {
                parameterDescriptions.Add( SetAction( NewExpandParameter( context ), apiDescription ) );
            }

            if ( singleResult )
            {
                return;
            }

            if ( options.HasFlag( Filter ) )
            {
                parameterDescriptions.Add( SetAction( NewFilterParameter( context ), apiDescription ) );
            }

            if ( options.HasFlag( OrderBy ) )
            {
                parameterDescriptions.Add( SetAction( NewOrderByParameter( context ), apiDescription ) );
            }

            if ( options.HasFlag( Top ) )
            {
                parameterDescriptions.Add( SetAction( NewTopParameter( context ), apiDescription ) );
            }

            if ( options.HasFlag( Skip ) )
            {
                parameterDescriptions.Add( SetAction( NewSkipParameter( context ), apiDescription ) );
            }

            if ( options.HasFlag( Count ) )
            {
                parameterDescriptions.Add( SetAction( NewCountParameter( context ), apiDescription ) );
            }
        }

        /// <summary>
        /// Creates a new API parameter description.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="description">The parameter description.</param>
        /// <param name="type">The parameter value type.</param>
        /// <param name="defaultValue">The parameter default value, if any.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewParameterDescription( string name, string description, Type type, object? defaultValue = default )
        {
            return new ApiParameterDescription()
            {
                Documentation = description,
                Name = name,
                ParameterDescriptor = new ODataQueryOptionParameterDescriptor( name, type, defaultValue ),
                Source = FromUri,
            };
        }

        static bool IsSingleResult( ApiDescription description, out Type? resultType )
        {
            var responseType = description.ResponseDescription.ResponseType;

            if ( responseType == null )
            {
                resultType = default;
                return true;
            }

            responseType = responseType.ExtractInnerType();

            if ( responseType.IsEnumerable( out resultType ) )
            {
                return false;
            }

            resultType = responseType;
            return true;
        }

        static ApiParameterDescription SetAction( ApiParameterDescription parameter, ApiDescription apiDescription )
        {
            var action = apiDescription.ActionDescriptor;
            var descriptor = parameter.ParameterDescriptor;

            descriptor.ActionDescriptor = action;
            descriptor.Configuration = action.Configuration;

            if ( descriptor is ODataQueryOptionParameterDescriptor odataDescriptor && apiDescription.Route is ODataRoute route )
            {
                var container = apiDescription.ActionDescriptor.Configuration.GetODataRootContainer( route );
                var omitPrefix = container.GetRequiredService<ODataUriResolver>().EnableNoDollarQueryOptions;

                if ( omitPrefix )
                {
                    odataDescriptor.SetPrefix( string.Empty );
                }
            }

            return parameter;
        }

        static bool IsSupported( ApiDescription apiDescription )
        {
            return apiDescription.HttpMethod.Method.ToUpperInvariant() switch
            {
                "GET" => true,
                "POST" => apiDescription.Operation()?.IsAction() == true,
                _ => false,
            };
        }
    }
}