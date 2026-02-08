// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http.Description;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Description;
using static System.Globalization.CultureInfo;

/// <summary>
/// Provides extension methods for the <see cref="ApiDescription"/> class.
/// </summary>
public static class ApiDescriptionExtensions
{
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the API version for.</param>
    extension( ApiDescription apiDescription )
    {
        /// <summary>
        /// Gets the API version associated with the API description.
        /// </summary>
        /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        /// <remarks>This method always returns <c>null</c> unless the <paramref name="apiDescription">API description</paramref>
        /// is of type <see cref="VersionedApiDescription"/>.</remarks>
        public ApiVersion? ApiVersion
        {
            get
            {
                if ( apiDescription is VersionedApiDescription versionedApiDescription )
                {
                    return versionedApiDescription.ApiVersion;
                }

                return default;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the associated API description is deprecated.
        /// </summary>
        /// <returns><c>True</c> if the <see cref="ApiDescription">API description</see> is deprecated; otherwise, <c>false</c>.</returns>
        /// <remarks>This method always returns <c>false</c> unless the <paramref name="apiDescription">API description</paramref>
        /// is of type <see cref="VersionedApiDescription"/>.</remarks>
        public bool IsDeprecated
        {
            get
            {
                if ( apiDescription is VersionedApiDescription versionedApiDescription )
                {
                    return versionedApiDescription.IsDeprecated;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the group name associated with the API description.
        /// </summary>
        /// <returns>The associated group name or <c>null</c>.</returns>
        /// <remarks>This method always returns <c>null</c> unless the <paramref name="apiDescription">API description</paramref>
        /// is of type <see cref="VersionedApiDescription"/>.</remarks>
        public string? GroupName
        {
            get
            {
                if ( apiDescription is VersionedApiDescription versionedApiDescription )
                {
                    return versionedApiDescription.GroupName;
                }

                return default;
            }
        }

        /// <summary>
        /// Gets the unique API description identifier.
        /// </summary>
        /// <returns>The unique identifier of the API description.</returns>
        /// <remarks>If the <paramref name="apiDescription">API description</paramref> is of type <see cref="VersionedApiDescription"/>
        /// the return value will be in the format of "{<see cref="ApiDescription.ID"/>}-{<see cref="VersionedApiDescription.ApiVersion"/>}";
        /// otherwise, the return value will be "{<see cref="ApiDescription.ID"/>}".</remarks>
        public string UniqueID
        {
            get
            {
                if ( apiDescription == null )
                {
                    throw new ArgumentNullException( nameof( apiDescription ) );
                }

                if ( apiDescription is VersionedApiDescription versionedApiDescription )
                {
                    return $"{versionedApiDescription.ID}-{versionedApiDescription.ApiVersion}";
                }

                return apiDescription.ID;
            }
        }

        /// <summary>
        /// Attempts to update the relate path of the specified API description and remove the corresponding parameter according to the specified options.
        /// </summary>
        /// <param name="options">The current <see cref="ApiExplorerOptions">API Explorer options</see>.</param>
        /// <returns>True if the API description was updated; otherwise, false.</returns>
        public bool TryUpdateRelativePathAndRemoveApiVersionParameter( ApiExplorerOptions options )
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            if ( options == null )
            {
                throw new ArgumentNullException( nameof( options ) );
            }

            if ( !options.SubstituteApiVersionInUrl || apiDescription is not VersionedApiDescription versionedApiDescription )
            {
                return false;
            }

            var relativePath = apiDescription.RelativePath;

            if ( string.IsNullOrEmpty( relativePath ) )
            {
                return false;
            }

            var parameters = versionedApiDescription.ParameterDescriptions;
            var parameter = parameters.FirstOrDefault( p => p.ParameterDescriptor is ApiVersionParameterDescriptor pd && pd.FromPath );

            if ( parameter == null )
            {
                return false;
            }

            var token = '{' + parameter.ParameterDescriptor.ParameterName + '}';
            var value = versionedApiDescription.ApiVersion.ToString( options.SubstitutionFormat, InvariantCulture );
            var newRelativePath = relativePath.Replace( token, value );

            if ( relativePath == newRelativePath )
            {
                return false;
            }

            apiDescription.RelativePath = newRelativePath;
            parameters.Remove( parameter );
            return true;
        }
    }

    /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to get the property from.</param>
    extension( VersionedApiDescription apiDescription )
    {
        /// <summary>
        /// Gets a property of the specified type from the API description.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of property to retrieve.</typeparam>
        /// <returns>The value of the property, if present; otherwise, the default value of <typeparamref name="T"/>.</returns>
        public T GetProperty<T>()
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            if ( apiDescription.Properties.TryGetValue( typeof( T ), out var value ) )
            {
                return (T) value;
            }

            return default!;
        }

        /// <summary>
        /// Sets a property of the specified type on the API description.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of property to set.</typeparam>
        /// <param name="value">The value to add or update.</param>
        public void SetProperty<T>( T value )
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            var key = typeof( T );

            if ( !key.IsValueType && value is null )
            {
                apiDescription.Properties.Remove( key );
            }
            else
            {
                apiDescription.Properties[key] = value!;
            }
        }
    }
}