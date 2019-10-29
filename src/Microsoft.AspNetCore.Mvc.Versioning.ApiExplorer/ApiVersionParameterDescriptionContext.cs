namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionParameterLocation;
    using static System.Linq.Enumerable;
    using static System.StringComparison;

    /// <summary>
    /// Represents an object that contains API version parameter descriptions.
    /// </summary>
    public class ApiVersionParameterDescriptionContext : IApiVersionParameterDescriptionContext
    {
        readonly List<ApiParameterDescription> parameters = new List<ApiParameterDescription>( 1 );
        readonly Lazy<bool> versionNeutral;
        bool optional;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionParameterDescriptionContext"/> class.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription"/> to provide API version parameter descriptions for.</param>
        /// <param name="apiVersion">The current API version.</param>
        /// <param name="modelMetadata">The <see cref="ModelMetadata">metadata</see> for the API version parameters.</param>
        /// <param name="options">The configured <see cref="ApiExplorerOptions">API explorer options</see>.</param>
        [CLSCompliant( false )]
        public ApiVersionParameterDescriptionContext(
            ApiDescription apiDescription,
            ApiVersion apiVersion,
            ModelMetadata modelMetadata,
            ApiExplorerOptions options )
        {
            if ( options == null )
            {
                throw new ArgumentNullException( nameof( options ) );
            }

            ApiDescription = apiDescription;
            ApiVersion = apiVersion;
            ModelMetadata = modelMetadata;
            Options = options;
            optional = options.AssumeDefaultVersionWhenUnspecified && apiVersion == options.DefaultApiVersion;
            versionNeutral = new Lazy<bool>( () => ApiDescription.ActionDescriptor.GetApiVersionModel().IsApiVersionNeutral );
        }

        /// <summary>
        /// Gets the associated API description.
        /// </summary>
        /// <value>The associated <see cref="ApiDescription">API description</see>.</value>
        [CLSCompliant( false )]
        protected ApiDescription ApiDescription { get; }

        /// <summary>
        /// Gets the associated API version.
        /// </summary>
        /// <value>The associated <see cref="ApiVersion">API version</see>.</value>
        protected ApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the current API is version-neutral.
        /// </summary>
        /// <value>True if the current API is version-neutral; otherwise, false.</value>
        protected bool IsApiVersionNeutral => versionNeutral.Value;

        /// <summary>
        /// Gets the model metadata for API version parameters.
        /// </summary>
        /// <value>The <see cref="ModelMetadata">model metadata</see> for the API version parameter.</value>
        [CLSCompliant( false )]
        protected ModelMetadata ModelMetadata { get; }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The configured <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ApiExplorerOptions Options { get; }

        bool HasPathParameter
        {
            get
            {
                var query = from description in ApiDescription.ParameterDescriptions
                            where description.Source == BindingSource.Path &&
                                  description.ModelMetadata?.DataTypeName == nameof( ApiVersion )
                            let constraints = description.RouteInfo?.Constraints ?? Empty<IRouteConstraint>()
                            where constraints.OfType<ApiVersionRouteConstraint>().Any()
                            select description;

                return query.Any();
            }
        }

        /// <summary>
        /// Adds an API version parameter with the specified name, from the specified location.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="location">One of the <see cref="ApiVersionParameterLocation"/> values.</param>
        public virtual void AddParameter( string name, ApiVersionParameterLocation location )
        {
            Action<string> add;

            switch ( location )
            {
                case Query:
                    add = AddQueryString;
                    break;
                case Header:
                    add = AddHeader;
                    break;
                case Path:
                    UpdateUrlSegment();
                    return;
                case MediaTypeParameter:
                    add = AddMediaTypeParameter;
                    break;
                default:
                    return;
            }

            if ( Options.AddApiVersionParametersWhenVersionNeutral || !IsApiVersionNeutral )
            {
                add( name );
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a query string parameter.
        /// </summary>
        /// <param name="name">The name of the query string parameter.</param>
        protected virtual void AddQueryString( string name )
        {
            if ( !HasPathParameter )
            {
                ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, BindingSource.Query ) );
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a header.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        protected virtual void AddHeader( string name )
        {
            if ( !HasPathParameter )
            {
                ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, BindingSource.Header ) );
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a route parameter in a URL segment.
        /// </summary>
        protected virtual void UpdateUrlSegment()
        {
            var query = from description in ApiDescription.ParameterDescriptions
                        let routeInfo = description.RouteInfo
                        where routeInfo != null
                        let constraints = routeInfo.Constraints ?? Empty<IRouteConstraint>()
                        where constraints.OfType<ApiVersionRouteConstraint>().Any()
                        select description;
            var parameter = query.FirstOrDefault();

            if ( parameter == null )
            {
                return;
            }

            parameter.IsRequired = true;
            parameter.DefaultValue = ApiVersion.ToString();
            parameter.ModelMetadata = ModelMetadata;
            parameter.Type = ModelMetadata.ModelType;
            parameter.RouteInfo.IsOptional = false;
            parameter.RouteInfo.DefaultValue = parameter.DefaultValue;

            if ( parameter.ParameterDescriptor == null )
            {
                parameter.ParameterDescriptor = new ParameterDescriptor()
                {
                    Name = parameter.Name,
                    ParameterType = typeof( ApiVersion ),
                };
            }

            RemoveAllParametersExcept( parameter );
        }

        /// <summary>
        /// Adds the description for an API version expressed as a media type parameter.
        /// </summary>
        /// <param name="name">The name of the media type parameter.</param>
        protected virtual void AddMediaTypeParameter( string name )
        {
            var requestFormats = ApiDescription.SupportedRequestFormats.ToArray();
            var responseTypes = ApiDescription.SupportedResponseTypes.ToArray();
            var parameter = $"{name}={ApiVersion}";

            ApiDescription.SupportedRequestFormats.Clear();
            ApiDescription.SupportedResponseTypes.Clear();

            foreach ( var requestFormat in requestFormats )
            {
                var newRequestFormat = requestFormat;

                if ( newRequestFormat.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) < 0 )
                {
                    newRequestFormat = newRequestFormat.Clone();
                    newRequestFormat.MediaType += "; " + parameter;
                }

                ApiDescription.SupportedRequestFormats.Add( newRequestFormat );
            }

            foreach ( var responseType in responseTypes )
            {
                var newResponseType = responseType;

                if ( !newResponseType.ApiResponseFormats.All( f => f.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) > 0 ) )
                {
                    newResponseType = newResponseType.Clone();

                    foreach ( var responseFormat in newResponseType.ApiResponseFormats )
                    {
                        if ( responseFormat.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) < 0 )
                        {
                            responseFormat.MediaType += "; " + parameter;
                        }
                    }
                }

                ApiDescription.SupportedResponseTypes.Add( newResponseType );
            }
        }

        ApiParameterDescription NewApiVersionParameter( string name, BindingSource source )
        {
            var parameter = new ApiParameterDescription()
            {
                DefaultValue = ApiVersion.ToString(),
                IsRequired = !optional,
                ModelMetadata = ModelMetadata,
                Name = name,
                ParameterDescriptor = new ParameterDescriptor()
                {
                    Name = name,
                    ParameterType = typeof( ApiVersion ),
                },
                Source = source,
                Type = ModelMetadata.ModelType,
            };

            if ( source == BindingSource.Path )
            {
                parameter.IsRequired = true;
                parameter.RouteInfo = new ApiParameterRouteInfo()
                {
                    DefaultValue = ApiVersion.ToString(),
                    IsOptional = false,
                };
            }

            optional = true;
            parameters.Add( parameter );

            return parameter;
        }

        void RemoveAllParametersExcept( ApiParameterDescription parameter )
        {
            // in a scenario where multiple api version parameters are allowed, we can remove all other parameters because
            // the api version must be specified in the path. this will avoid unwanted, duplicate api version parameters
            var collections = new ICollection<ApiParameterDescription>[] { ApiDescription.ParameterDescriptions, parameters };

            foreach ( var collection in collections )
            {
                var otherParameters = collection.Where( p => p != parameter ).ToArray();

                foreach ( var otherParameter in otherParameters )
                {
                    if ( otherParameter.ModelMetadata?.DataTypeName == nameof( ApiVersion ) )
                    {
                        collection.Remove( otherParameter );
                    }
                }
            }
        }
    }
}