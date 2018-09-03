namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpActionDescriptor"/> class.
    /// </summary>
    public static class HttpActionDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        public static ApiVersionModel GetApiVersionModel( this HttpActionDescriptor actionDescriptor )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            return actionDescriptor.GetProperty<ApiVersionModel>() ?? new ApiVersionModel( actionDescriptor );
        }

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

        internal static bool IsAttributeRouted( this HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( actionDescriptor != null );

            actionDescriptor.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
            return value ?? false;
        }

        internal static T GetProperty<T>( this HttpActionDescriptor actionDescriptor ) =>
            actionDescriptor.Properties.TryGetValue( typeof( T ), out T value ) ? value : default;

        internal static void SetProperty<T>( this HttpActionDescriptor actionDescriptor, T value ) =>
            actionDescriptor.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );

#pragma warning disable  CA1801 // Review unused parameters; intentional for type parameter
        internal static void RemoveProperty<T>( this HttpActionDescriptor actionDescriptor, T value ) =>
            actionDescriptor.Properties.TryRemove( typeof( T ), out _ );
#pragma warning restore CA1801
    }
}