namespace Microsoft.OData.Edm
{
#if !WEBAPI
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using Microsoft.Extensions.DependencyInjection;
#if WEBAPI
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if WEBAPI
    using System.Net.Http;
    using System.Web.Http;
    using HttpRequest = System.Net.Http.HttpRequestMessage;
#endif

    /// <summary>
    /// Represents an <see cref="IEdmModelSelector">EDM model selector</see>.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public class EdmModelSelector : IEdmModelSelector
    {
        readonly ApiVersion maxVersion;
        readonly IApiVersionSelector selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmModelSelector"/> class.
        /// </summary>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">models</see> to select from.</param>
        /// <param name="defaultApiVersion">The default <see cref="ApiVersion">API version</see>.</param>
        [Obsolete( "This constructor will be removed in the next major version. Use the constructor with IApiVersionSelector instead." )]
        public EdmModelSelector( IEnumerable<IEdmModel> models, ApiVersion defaultApiVersion )
            : this( models, defaultApiVersion, new ConstantApiVersionSelector( defaultApiVersion ) ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmModelSelector"/> class.
        /// </summary>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">models</see> to select from.</param>
        /// <param name="defaultApiVersion">The default <see cref="ApiVersion">API version</see>.</param>
        /// <param name="apiVersionSelector">The <see cref="IApiVersionSelector">selector</see> used to choose API versions.</param>
        public EdmModelSelector( IEnumerable<IEdmModel> models, ApiVersion defaultApiVersion, IApiVersionSelector apiVersionSelector )
        {
            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            if ( defaultApiVersion == null )
            {
                throw new ArgumentNullException( nameof( defaultApiVersion ) );
            }

            selector = apiVersionSelector ?? throw new ArgumentNullException( nameof( apiVersionSelector ) );

            List<ApiVersion> versions;
            Dictionary<ApiVersion, IEdmModel> collection;

            switch ( models )
            {
                case IList<IEdmModel> list:
                    versions = new( list.Count );
                    collection = new( list.Count );

                    for ( var i = 0; i < list.Count; i++ )
                    {
                        AddVersionFromModel( list[i], versions, collection );
                    }

                    break;
                case IReadOnlyList<IEdmModel> list:
                    versions = new( list.Count );
                    collection = new( list.Count );

                    for ( var i = 0; i < list.Count; i++ )
                    {
                        AddVersionFromModel( list[i], versions, collection );
                    }

                    break;
                default:
                    versions = new();
                    collection = new();

                    foreach ( var model in models )
                    {
                        AddVersionFromModel( model, versions, collection );
                    }
#if !WEBAPI
                    collection.TrimExcess();
#endif
                    break;
            }

            versions.Sort();
#pragma warning disable IDE0056 // Use index operator (cannot be used in web api)
            maxVersion = versions.Count == 0 ? defaultApiVersion : versions[versions.Count - 1];
#pragma warning restore IDE0056
            ApiVersions = versions.ToArray();
            Models = collection;
        }

        /// <inheritdoc />
        public IReadOnlyList<ApiVersion> ApiVersions { get; }

        /// <summary>
        /// Gets the collection of EDM models.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="IEdmModel">EDM models</see>.</value>
        protected IDictionary<ApiVersion, IEdmModel> Models { get; }

        /// <inheritdoc />
        public virtual bool Contains( ApiVersion? apiVersion ) => apiVersion != null && Models.ContainsKey( apiVersion );

        /// <inheritdoc />
        public virtual IEdmModel? SelectModel( ApiVersion? apiVersion ) =>
            apiVersion != null && Models.TryGetValue( apiVersion, out var model ) ? model : default;

        /// <inheritdoc />
        public virtual IEdmModel? SelectModel( IServiceProvider serviceProvider )
        {
            if ( Models.Count == 0 )
            {
                return default;
            }

            var request = serviceProvider.GetService<HttpRequest>();

            if ( request is null )
            {
                return Models[maxVersion];
            }

            var version = request
#if !WEBAPI
                .HttpContext
#endif
                .GetRequestedApiVersion();

            if ( version is null )
            {
                var model = new ApiVersionModel( ApiVersions, Enumerable.Empty<ApiVersion>() );

                if ( ( version = selector.SelectVersion( request, model ) ) is null )
                {
                    return Models[maxVersion];
                }
            }

            return Models.TryGetValue( version, out var edm ) ? edm : Models[maxVersion];
        }

        static void AddVersionFromModel( IEdmModel model, IList<ApiVersion> versions, IDictionary<ApiVersion, IEdmModel> collection )
        {
            var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model );

            if ( annotation == null )
            {
                throw new ArgumentException( LocalSR.MissingAnnotation.FormatDefault( typeof( ApiVersionAnnotation ).Name ) );
            }

            var version = annotation.ApiVersion;

            collection.Add( version, model );
            versions.Add( version );
        }
    }
}