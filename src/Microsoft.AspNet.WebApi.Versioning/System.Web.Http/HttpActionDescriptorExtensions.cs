namespace System.Web.Http
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Controllers;
    using static Microsoft.Web.Http.Versioning.ApiVersionMapping;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpActionDescriptor"/> class.
    /// </summary>
    public static class HttpActionDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        public static ApiVersionModel GetApiVersionModel( this HttpActionDescriptor action ) => action.GetApiVersionModel( Explicit );

        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <param name="mapping">One or more of the <see cref="ApiVersionMapping"/> values.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        public static ApiVersionModel GetApiVersionModel( this HttpActionDescriptor action, ApiVersionMapping mapping )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            switch ( mapping )
            {
                case Explicit:
                    return action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;
                case Implicit:
                    return action.ControllerDescriptor.GetApiVersionModel();
                case Explicit | Implicit:

                    var model = action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

                    if ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Count > 0 )
                    {
                        return model;
                    }

                    var implicitModel = action.ControllerDescriptor.GetApiVersionModel();

                    return new ApiVersionModel(
                        implicitModel.DeclaredApiVersions,
                        model.SupportedApiVersions,
                        model.DeprecatedApiVersions,
                        Empty<ApiVersion>(),
                        Empty<ApiVersion>() );
            }

            return ApiVersionModel.Empty;
        }

        /// <summary>
        /// Returns a value indicating whether the provided action maps to the specified API version.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>One of the <see cref="ApiVersionMapping"/> values.</returns>
        public static ApiVersionMapping MappingTo( this HttpActionDescriptor action, ApiVersion? apiVersion )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            var model = action.GetApiVersionModel();

            if ( model.IsApiVersionNeutral || ( apiVersion != null && model.DeclaredApiVersions.Contains( apiVersion ) ) )
            {
                return Explicit;
            }
            else if ( model.DeclaredApiVersions.Count == 0 )
            {
                model = action.ControllerDescriptor.GetApiVersionModel();

                if ( apiVersion != null && model.DeclaredApiVersions.Contains( apiVersion ) )
                {
                    return Implicit;
                }
            }

            return None;
        }

        /// <summary>
        /// Returns a value indicating whether the provided action maps to the specified API version.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>True if the <paramref name="action"/> explicitly or implicitly maps to the specified
        /// <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
        public static bool IsMappedTo( this HttpActionDescriptor action, ApiVersion apiVersion ) => action.MappingTo( apiVersion ) > None;

        internal static bool IsAttributeRouted( this HttpActionDescriptor action ) =>
            action.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value ) && ( value ?? false );

        internal static T GetProperty<T>( this HttpActionDescriptor action ) =>
            action.Properties.TryGetValue( typeof( T ), out T value ) ? value : default;

        internal static void SetProperty<T>( this HttpActionDescriptor action, T value ) =>
            action.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );
    }
}