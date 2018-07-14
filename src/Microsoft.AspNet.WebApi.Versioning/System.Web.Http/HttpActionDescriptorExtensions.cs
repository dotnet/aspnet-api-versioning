namespace System.Web.Http
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpActionDescriptor"/> class.
    /// </summary>
    public static class HttpActionDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
        const string ApiVersionInfoKey = "MS_ApiVersionInfo";

        internal static bool IsAttributeRouted( this HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( actionDescriptor != null );

            actionDescriptor.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
            return value ?? false;
        }

        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ApiVersionModel GetApiVersionModel( this HttpActionDescriptor actionDescriptor )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            return (ApiVersionModel) actionDescriptor.Properties.GetOrAdd( ApiVersionInfoKey, key => new ApiVersionModel( actionDescriptor ) );
        }

        internal static void SetApiVersionModel( this HttpActionDescriptor actionDescriptor, ApiVersionModel model ) =>
            actionDescriptor.Properties.AddOrUpdate( ApiVersionInfoKey, model, ( key, value ) => model );

        /// <summary>
        /// Gets a value indicating whether the action is API version neutral.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <returns>True if the action is API version neutral (e.g. "unaware"); otherwise, false.</returns>
        public static bool IsApiVersionNeutral( this HttpActionDescriptor actionDescriptor ) => actionDescriptor.GetApiVersionModel().IsApiVersionNeutral;

        /// <summary>
        /// Gets the API versions declared by the action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// declared by the action.</returns>
        public static IReadOnlyList<ApiVersion> GetApiVersions( this HttpActionDescriptor actionDescriptor ) => actionDescriptor.GetApiVersionModel().DeclaredApiVersions;
    }
}