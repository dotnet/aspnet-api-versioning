namespace Microsoft.OData.Edm
{
#if !WEBAPI
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
#endif
    using Microsoft.Extensions.DependencyInjection;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;
    using System.Collections.Generic;
#if WEBAPI
    using System.Net.Http;
    using System.Web.Http;
#endif

    /// <summary>
    /// Represents an <see cref="IEdmModelSelector">EDM model selector</see>.
    /// </summary>
#if WEBAPI
    [CLSCompliant( false )]
#endif
    public class EdmModelSelector : IEdmModelSelector
    {
        readonly ApiVersion maxVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmModelSelector"/> class.
        /// </summary>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">models</see> to select from.</param>
        /// <param name="defaultApiVersion">The default <see cref="ApiVersion">API version</see>.</param>
        public EdmModelSelector( IEnumerable<IEdmModel> models, ApiVersion defaultApiVersion )
        {
            var versions = new List<ApiVersion>();
            var collection = new Dictionary<ApiVersion, IEdmModel>();

            foreach ( var model in models ?? throw new ArgumentNullException( nameof( models ) ) )
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

            versions.Sort();
#pragma warning disable IDE0056 // Use index operator (cannot be used in web api)
            maxVersion = versions.Count == 0 ? defaultApiVersion : versions[versions.Count - 1];
#pragma warning restore IDE0056
#if !WEBAPI
            collection.TrimExcess();
#endif
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

#if WEBAPI
            var version = serviceProvider.GetService<HttpRequestMessage>()?.GetRequestedApiVersion();
#else
            var version = serviceProvider.GetService<HttpRequest>()?.HttpContext.GetRequestedApiVersion();
#endif

            return version != null && Models.TryGetValue( version, out var model ) ? model : Models[maxVersion];
        }
    }
}