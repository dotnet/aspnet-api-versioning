namespace System.Web.Http
{
    using Collections.Generic;
    using Controllers;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpControllerDescriptor"/> class.
    /// </summary>
    public static class HttpControllerDescriptorExtensions
    {
        private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
        private const string ApiVersionInfoKey = "MS_ApiVersionInfo";

        internal static bool IsAttributeRouted( this HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            var value = default( bool? );
            controllerDescriptor.Properties.TryGetValue( AttributeRoutedPropertyKey, out value );
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

            var versionInfo = default( ApiVersionModel );
            return controllerDescriptor.Properties.TryGetValue( ApiVersionInfoKey, out versionInfo ) ? versionInfo : new ApiVersionModel( controllerDescriptor );
        }

        internal static void SetApiVersionInfo( this HttpControllerDescriptor controllerDescriptor, ApiVersionModel versionInfo ) =>
            controllerDescriptor.Properties.AddOrUpdate( ApiVersionInfoKey, key => new ApiVersionModel( controllerDescriptor, versionInfo ), ( key, value ) => ( (ApiVersionModel) value ).Aggregate( versionInfo ) );

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
        /// <remarks>The declared API versions are constrainted to the versions declared explicitly by the specified controller.</remarks>
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
