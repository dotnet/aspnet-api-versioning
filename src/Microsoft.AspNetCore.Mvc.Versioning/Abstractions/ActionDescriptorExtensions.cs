namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using System.Linq;
    using System.Text;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Provides extension methods for the <see cref="ActionDescriptor"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ActionDescriptorExtensions
    {
        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        public static ApiVersionModel GetApiVersionModel( this ActionDescriptor action ) => action.GetApiVersionModel( Explicit );

        /// <summary>
        /// Gets the API version information associated with a action.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="mapping">One or more of the <see cref="ApiVersionMapping"/> values.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the action.</returns>
        public static ApiVersionModel GetApiVersionModel( this ActionDescriptor action, ApiVersionMapping mapping )
        {
            switch ( mapping )
            {
                case Explicit:
                    return action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;
                case Implicit:
                    return action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;
                case Explicit | Implicit:

                    var model = action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

                    if ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Count > 0 )
                    {
                        return model;
                    }

                    var implicitModel = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

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
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>One of the <see cref="ApiVersionMapping"/> values.</returns>
        public static ApiVersionMapping MappingTo( this ActionDescriptor action, ApiVersion? apiVersion )
        {
            var model = action.GetApiVersionModel();

            if ( model.IsApiVersionNeutral || ( apiVersion != null && model.DeclaredApiVersions.Contains( apiVersion ) ) )
            {
                return Explicit;
            }
            else if ( model.DeclaredApiVersions.Count == 0 )
            {
                var parentModel = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

                if ( parentModel != null && ( apiVersion != null && parentModel.DeclaredApiVersions.Contains( apiVersion ) ) )
                {
                    return Implicit;
                }
            }

            return None;
        }

        /// <summary>
        /// Returns a value indicating whether the provided action maps to the specified API version.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>True if the <paramref name="action"/> explicitly or implicitly maps to the specified
        /// <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
        public static bool IsMappedTo( this ActionDescriptor action, ApiVersion? apiVersion ) => action.MappingTo( apiVersion ) > None;

        internal static string ExpandSignature( this ActionDescriptor action )
        {
            if ( !( action is ControllerActionDescriptor controllerAction ) )
            {
                return action.DisplayName;
            }

            var signature = new StringBuilder();
            var controllerType = controllerAction.ControllerTypeInfo;

            signature.Append( controllerType.GetTypeDisplayName() );
            signature.Append( '.' );
            signature.Append( controllerAction.MethodInfo.Name );
            signature.Append( '(' );

            using ( var parameter = action.Parameters.GetEnumerator() )
            {
                if ( parameter.MoveNext() )
                {
                    var parameterType = parameter.Current.ParameterType;

                    signature.Append( parameterType.GetTypeDisplayName( false ) );

                    while ( parameter.MoveNext() )
                    {
                        parameterType = parameter.Current.ParameterType;
                        signature.Append( ", " );
                        signature.Append( parameterType.GetTypeDisplayName( false ) );
                    }
                }
            }

            signature.Append( ") (" );
            signature.Append( controllerType.Assembly.GetName().Name );
            signature.Append( ')' );

            return signature.ToString();
        }
    }
}