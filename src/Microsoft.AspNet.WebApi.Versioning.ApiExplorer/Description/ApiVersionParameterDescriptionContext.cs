﻿namespace Microsoft.Web.Http.Description
{
    using Microsoft.Web.Http.Versioning;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http.Description;
    using static Microsoft.Web.Http.Versioning.ApiVersionParameterLocation;
    using static System.Web.Http.Description.ApiParameterSource;
    using static System.StringComparison;
    using System.Web.Http;

    /// <summary>
    /// Represents an object that contains API version parameter descriptions.
    /// </summary>
    public class ApiVersionParameterDescriptionContext : IApiVersionParameterDescriptionContext
    {
        readonly List<ApiParameterDescription> parameters = new List<ApiParameterDescription>( 1 );
        bool optional;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionParameterDescriptionContext"/> class.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription"/> to provide API version parameter descriptions for.</param>
        /// <param name="apiVersion">The current API version.</param>
        /// <param name="options">The configured <see cref="ApiExplorerOptions">API explorer options</see>.</param>
        public ApiVersionParameterDescriptionContext(
            ApiDescription apiDescription,
            ApiVersion apiVersion,
            ApiExplorerOptions options )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( options, nameof( options ) );

            ApiDescription = apiDescription;
            ApiVersion = apiVersion;
            Options = options;
            optional = options.AssumeDefaultVersionWhenUnspecified && apiVersion == options.DefaultApiVersion;
        }

        /// <summary>
        /// Gets the associated API description.
        /// </summary>
        /// <value>The associated <see cref="ApiDescription">API description</see>.</value>
        protected ApiDescription ApiDescription { get; }

        /// <summary>
        /// Gets the associated API version.
        /// </summary>
        /// <value>The associated <see cref="ApiVersion">API version</see>.</value>
        protected ApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The configured <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ApiExplorerOptions Options { get; }

        bool HasPathParameter
        {
            get
            {
                return ApiDescription.ParameterDescriptions
                                     .Select( p => p.ParameterDescriptor )
                                     .OfType<ApiVersionParameterDescriptor>()
                                     .Where( d => d.FromPath )
                                     .Any();
            }
        }

        /// <summary>
        /// Adds an API version parameter with the specified name, from the specified location.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="location">One of the <see cref="ApiVersionParameterLocation"/> values.</param>
        public virtual void AddParameter( string name, ApiVersionParameterLocation location )
        {
            switch ( location )
            {
                case Query:
                    AddQueryString( name );
                    break;
                case Header:
                    AddHeader( name );
                    break;
                case Path:
                    UpdateUrlSegment();
                    break;
                case MediaTypeParameter:
                    AddMediaTypeParameter( name );
                    break;
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a query string parameter.
        /// </summary>
        /// <param name="name">The name of the query string parameter.</param>
        protected virtual void AddQueryString( string name )
        {
            Arg.NotNullOrEmpty( name, nameof( name ) );

            if ( !HasPathParameter )
            {
                ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, FromUri ) );
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a header.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        protected virtual void AddHeader( string name )
        {
            Arg.NotNullOrEmpty( name, nameof( name ) );

            if ( !HasPathParameter )
            {
                ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, Unknown ) );
            }
        }

        /// <summary>
        /// Adds the description for an API version expressed as a header.
        /// </summary>
        protected virtual void UpdateUrlSegment()
        {
            var parameter = ApiDescription.ParameterDescriptions.FirstOrDefault( p => p.Source == FromUri && p.ParameterDescriptor == null );

            if ( parameter == null )
            {
                return;
            }

            var action = ApiDescription.ActionDescriptor;

            parameter.Documentation = Options.DefaultApiVersionParameterDescription;
            parameter.ParameterDescriptor = new ApiVersionParameterDescriptor( parameter.Name, ApiVersion.ToString(), fromPath: true )
            {
                Configuration = action.Configuration,
                ActionDescriptor = action
            };
            RemoveAllParametersExcept( parameter );
        }

        /// <summary>
        /// Adds the description for an API version expressed as a media type parameter.
        /// </summary>
        /// <param name="name">The name of the media type parameter.</param>
        protected virtual void AddMediaTypeParameter( string name )
        {
            Arg.NotNullOrEmpty( name, nameof( name ) );

            var parameter = new NameValueHeaderValue( name, ApiVersion.ToString() );

            CloneFormattersAndAddMediaTypeParameter( parameter, ApiDescription.SupportedRequestBodyFormatters );
            CloneFormattersAndAddMediaTypeParameter( parameter, ApiDescription.SupportedResponseFormatters );
        }

        ApiParameterDescription NewApiVersionParameter( string name, ApiParameterSource source )
        {
            Contract.Requires( !string.IsNullOrEmpty( name ) );
            Contract.Ensures( Contract.Result<ApiParameterDescription>() != null );

            var action = ApiDescription.ActionDescriptor;
            var parameter = new ApiParameterDescription()
            {
                Name = name,
                Documentation = Options.DefaultApiVersionParameterDescription,
                ParameterDescriptor = new ApiVersionParameterDescriptor( name, ApiVersion.ToString(), optional )
                {
                    Configuration = action.Configuration,
                    ActionDescriptor = action
                },
                Source = source
            };

            optional = true;
            parameters.Add( parameter );

            return parameter;
        }

        void RemoveAllParametersExcept( ApiParameterDescription parameter )
        {
            // note: in a scenario where multiple api version parameters are allowed, we can remove all other parameters because
            // the api version must be specified in the path. this will avoid unwanted, duplicate api version parameters

            var collections = new ICollection<ApiParameterDescription>[] { ApiDescription.ParameterDescriptions, parameters };

            foreach ( var collection in collections )
            {
                var otherParameters = collection.Where( p => p != parameter ).ToArray();

                foreach ( var otherParameter in otherParameters )
                {
                    if ( otherParameter.ParameterDescriptor is ApiVersionParameterDescriptor )
                    {
                        collection.Remove( otherParameter );
                    }
                }
            }
        }

        static void CloneFormattersAndAddMediaTypeParameter( NameValueHeaderValue parameter, ICollection<MediaTypeFormatter> formatters )
        {
            Contract.Requires( parameter != null );
            Contract.Requires( formatters != null );

            var originalFormatters = formatters.ToArray();

            formatters.Clear();

            foreach ( var originalFormatter in originalFormatters )
            {
                // note: we have to clone the media type formatter in order to generate different
                // media type parameters for each api version
                var formatter = originalFormatter.Clone();

                foreach ( var mediaType in formatter.SupportedMediaTypes )
                {
                    if ( !mediaType.Parameters.Any( p => p.Name.Equals( parameter.Name, OrdinalIgnoreCase ) ) )
                    {
                        mediaType.Parameters.Add( parameter );
                    }
                }

                formatters.Add( formatter );
            }
        }
    }
}