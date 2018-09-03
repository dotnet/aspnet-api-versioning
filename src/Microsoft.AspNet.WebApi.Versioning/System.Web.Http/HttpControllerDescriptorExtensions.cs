namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpControllerDescriptor"/> class.
    /// </summary>
    public static class HttpControllerDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
        const string RelatedControllerCandidatesKey = "MS_RelatedControllerCandidates";

        /// <summary>
        /// Gets the API version information associated with a controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
        public static ApiVersionModel GetApiVersionModel( this HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );
            return controllerDescriptor.GetProperty<ApiVersionModel>() ?? new ApiVersionModel( controllerDescriptor );
        }

        /// <summary>
        /// Gets a value indicating whether the controller is API version neutral.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>True if the controller is API version neutral (e.g. "unaware"); otherwise, false.</returns>
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
        /// controller would no longer indicate that it is an <see cref="GetImplementedApiVersions(HttpControllerDescriptor)">implemented version</see>.</remarks>
        public static IReadOnlyList<ApiVersion> GetDeprecatedApiVersions( this HttpControllerDescriptor controllerDescriptor ) => controllerDescriptor.GetApiVersionModel().DeprecatedApiVersions;

        internal static bool IsAttributeRouted( this HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            controllerDescriptor.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
            return value ?? false;
        }

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

        internal static T GetProperty<T>( this HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            if ( controllerDescriptor.Properties.TryGetValue( typeof( T ), out T value ) )
            {
                return value;
            }

            return default;
        }

        internal static void SetProperty<T>( this HttpControllerDescriptor controllerDescriptor, T value )
        {
            Contract.Requires( controllerDescriptor != null );

            controllerDescriptor.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );

            if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
            {
                foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                {
                    groupedControllerDescriptor.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );
                }
            }
        }
    }
}