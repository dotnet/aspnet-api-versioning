namespace System.Web.Http.Description
{
    using Microsoft;
    using Microsoft.Web.Http.Description;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the group name associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the group name for.</param>
        /// <returns>The associated group name or <c>null</c>.</returns>
        /// <remarks>This method always returns <c>null</c> unless the <paramref name="apiDescription">API description</paramref>
        /// is of type <see cref="VersionedApiDescription"/>.</remarks>
        public static string GetGroupName( this ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );

            if ( apiDescription is VersionedApiDescription versionedApiDescription )
            {
                return versionedApiDescription.GroupName;
            }

            return null;
        }
    }
}