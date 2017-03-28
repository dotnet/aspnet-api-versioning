namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the API version associated with the API description, if any.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the API version for.</param>
        /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        public static ApiVersion GetApiVersion( this ApiDescription apiDescription ) => apiDescription.GetProperty<ApiVersion>();

        /// <summary>
        /// Sets the API version associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to set the API version for.</param>
        /// <param name="apiVersion">The associated <see cref="ApiVersion">API version</see>.</param>
        [EditorBrowsable( Never )]
        public static void SetApiVersion( this ApiDescription apiDescription, ApiVersion apiVersion ) => apiDescription.SetProperty( apiVersion );

        /// <summary>
        /// Creates a shallow copy of the current API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to create a copy of.</param>
        /// <returns>A new <see cref="ApiDescription">API description</see>.</returns>
        public static ApiDescription Clone( this ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Contract.Ensures( Contract.Result<ApiDescription>() != null );

            var clone = new ApiDescription()
            {
                ActionDescriptor = apiDescription.ActionDescriptor,
                GroupName = apiDescription.GroupName,
                HttpMethod = apiDescription.HttpMethod,
                RelativePath = apiDescription.RelativePath
            };

            foreach ( var property in apiDescription.Properties )
            {
                clone.Properties.Add( property );
            }

            foreach ( var parameter in apiDescription.ParameterDescriptions )
            {
                clone.ParameterDescriptions.Add( parameter );
            }

            foreach ( var requestFormat in apiDescription.SupportedRequestFormats )
            {
                clone.SupportedRequestFormats.Add( requestFormat );
            }

            foreach ( var responseType in apiDescription.SupportedResponseTypes )
            {
                clone.SupportedResponseTypes.Add( responseType );
            }

            return clone;
        }
    }
}