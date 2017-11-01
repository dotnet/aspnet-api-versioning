namespace System.Web.Http.Description
{
    using Microsoft;
    using Microsoft.Web.Http.Description;
    using System.Diagnostics.Contracts;

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

        /// <summary>
        /// Gets the unique API description identifier.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the unique identifier for.</param>
        /// <returns>The unique identifier of the API description.</returns>
        /// <remarks>If the <paramref name="apiDescription">API description</paramref> is of type <see cref="VersionedApiDescription"/>
        /// the return value will be in the format of "{<see cref="ApiDescription.ID"/>}-{<see cref="VersionedApiDescription.ApiVersion"/>}";
        /// otherwise, the return value will be "{<see cref="ApiDescription.ID"/>}".</remarks>
        public static string GetUniqueID( this ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            if ( apiDescription is VersionedApiDescription versionedApiDescription )
            {
                return $"{versionedApiDescription.ID}-{versionedApiDescription.ApiVersion}";
            }

            return apiDescription.ID;
        }

        /// <summary>
        /// Gets a property of the specified type from the API description.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of property to retrieve.</typeparam>
        /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to get the property from.</param>
        /// <returns>The value of the property, if present; otherwise, the default value of <typeparamref name="T"/>.</returns>
        public static T GetProperty<T>( this VersionedApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );

            if ( apiDescription.Properties.TryGetValue( typeof( T ), out object value ) )
            {
                return (T) value;
            }

            return default( T );
        }

        /// <summary>
        /// Sets a property of the specified type on the API description.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of property to set.</typeparam>
        /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to set the property on.</param>
        /// <param name="value">The value to add or update.</param>
        public static void SetProperty<T>( this VersionedApiDescription apiDescription, T value )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            apiDescription.Properties[typeof( T )] = value;
        }
    }
}