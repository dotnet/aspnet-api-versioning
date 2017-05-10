namespace System.Web.Http
{
    using Collections.Generic;
    using Controllers;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq;
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpControllerDescriptor"/> class.
    /// </summary>
    public static class HttpControllerDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
        const string ApiVersionInfoKey = "MS_ApiVersionInfo";
        const string ConventionsApiVersionInfoKey = "MS_ConventionsApiVersionInfo";
        const string RelatedControllerCandidatesKey = "MS_RelatedControllerCandidates";

        internal static bool IsAttributeRouted( this HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            controllerDescriptor.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
            return value ?? false;
        }

        internal static bool HasApiVersionInfo( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.Properties.ContainsKey( ApiVersionInfoKey );

        internal static ApiVersionModel AggregateVersions( this IEnumerable<HttpControllerDescriptor> controllerDescriptors )
        {
            Contract.Requires( controllerDescriptors != null );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            using ( var iterator = controllerDescriptors.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return ApiVersionModel.Empty;
                }

                var version = iterator.Current.GetApiVersionModel();
                var otherVersions = new List<ApiVersionModel>();

                while ( iterator.MoveNext() )
                {
                    otherVersions.Add( iterator.Current.GetApiVersionModel() );
                }

                return version.Aggregate( otherVersions );
            }
        }

        /// <summary>
        /// Gets the API version information associated with a controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ApiVersionModel GetApiVersionModel( this HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            var properties = controllerDescriptor.Properties;

            if ( properties.TryGetValue( ApiVersionInfoKey, out ApiVersionModel versionInfo ) )
            {
                return versionInfo;
            }

            var options = controllerDescriptor.Configuration.GetApiVersioningOptions();

            if ( options.Conventions.Count == 0 )
            {
                return new ApiVersionModel( controllerDescriptor );
            }

            options.Conventions.ApplyTo( controllerDescriptor );
            return properties.TryGetValue( ConventionsApiVersionInfoKey, out versionInfo ) ? versionInfo : new ApiVersionModel( controllerDescriptor );
        }

        internal static void SetApiVersionModel( this HttpControllerDescriptor controllerDescriptor, ApiVersionModel model )
        {
            var properties = controllerDescriptor.Properties;

            properties.AddOrUpdate(
                ApiVersionInfoKey,
                key =>
                {
                    if ( properties.TryRemove( ConventionsApiVersionInfoKey, out var value ) )
                    {
                        return ( (ApiVersionModel) value ).Aggregate( model );
                    }

                    return new ApiVersionModel( controllerDescriptor, model );
                },
                ( key, value ) => ( (ApiVersionModel) value ).Aggregate( model ) );
        }

        internal static void SetConventionsApiVersionModel( this HttpControllerDescriptor controllerDescriptor, ApiVersionModel model ) =>
            controllerDescriptor.Properties.AddOrUpdate( ConventionsApiVersionInfoKey, model, ( key, currentModel ) => ( (ApiVersionModel) currentModel ).Aggregate( model ) );

        internal static void SetRelatedCandidates( this HttpControllerDescriptor controllerDescriptor, IEnumerable<HttpControllerDescriptor> value ) =>
            controllerDescriptor.Properties.AddOrUpdate( RelatedControllerCandidatesKey, value, ( key, oldValue ) => value );

        internal static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor )
        {
            if ( controllerDescriptor.Properties.TryGetValue( RelatedControllerCandidatesKey, out IEnumerable<HttpControllerDescriptor> relatedCandidates ) )
            {
                using ( var relatedControllerDescriptors = relatedCandidates.GetEnumerator() )
                {
                    if ( relatedControllerDescriptors.MoveNext() )
                    {
                        yield return controllerDescriptor;

                        do
                        {
                            if ( relatedControllerDescriptors.Current != controllerDescriptor )
                            {
                                yield return relatedControllerDescriptors.Current;
                            }
                        }
                        while ( relatedControllerDescriptors.MoveNext() );

                        yield break;
                    }
                }
            }

            if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
            {
                foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                {
                    yield return groupedControllerDescriptor;
                }
            }
            else
            {
                yield return controllerDescriptor;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the controller is API version neutral.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>True if the controller is API version neutral (e.g. "unaware"); otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static bool IsApiVersionNeutral( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().IsApiVersionNeutral;

        /// <summary>
        /// Gets the API versions declared by the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// declared by the controller.</returns>
        /// <remarks>The declared API versions are constrained to the versions declared explicitly by the specified controller.</remarks>
        public static IReadOnlyList<ApiVersion> GetDeclaredApiVersions( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().DeclaredApiVersions;

        /// <summary>
        /// Gets the API versions implemented by the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// implemented by the controller.</returns>
        /// <remarks>The implemented API versions include the supported and deprecated API versions.</remarks>
        public static IReadOnlyList<ApiVersion> GetImplementedApiVersions( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().ImplementedApiVersions;

        /// <summary>
        /// Gets the API versions supported by the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// supported by the controller.</returns>
        public static IReadOnlyList<ApiVersion> GetSupportedApiVersions( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().SupportedApiVersions;

        /// <summary>
        /// Gets the API versions deprecated by the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// deprecated by the controller.</returns>
        /// <remarks>A deprecated API version does not mean it is not supported by the controller. A deprecated API
        /// version is typically advertised six months or more before it becomes unsupported; in which case, the
        /// controller would no longer indicate that it is an <see cref="P:ImplementedVersions">implemented version</see>.</remarks>
        public static IReadOnlyList<ApiVersion> GetDeprecatedApiVersions( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().DeprecatedApiVersions;
    }
}