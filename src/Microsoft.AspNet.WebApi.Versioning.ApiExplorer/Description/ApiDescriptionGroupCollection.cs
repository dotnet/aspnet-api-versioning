namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Http.Description;

    /// <summary>
    /// Represents a collection of grouped API descriptions.
    /// </summary>
    public class ApiDescriptionGroupCollection : KeyedCollection<ApiVersion, ApiDescriptionGroup>
    {
        /// <summary>
        /// Gets the key for the specified item.
        /// </summary>
        /// <param name="item">The item to get the key for.</param>
        /// <returns>The key of the item.</returns>
        protected override ApiVersion GetKeyForItem( ApiDescriptionGroup item )
        {
            if ( item == null )
            {
                throw new ArgumentNullException( nameof( item ) );
            }

            return item.ApiVersion;
        }

        /// <summary>
        /// Gets or adds a new API description group for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to get a description group for.</param>
        /// <returns>A new or existing <see cref="ApiDescriptionGroup">API description group</see>.</returns>
        public virtual ApiDescriptionGroup GetOrAdd( ApiVersion apiVersion ) => GetOrAdd( apiVersion, v => v.ToString() );

        /// <summary>
        /// Gets or adds a new API description group for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to get a description group for.</param>
        /// <param name="formatName">The <see cref="Func{T, TResult}">function</see> used to format the name of a new description group.</param>
        /// <returns>A new or existing <see cref="ApiDescriptionGroup">API description group</see>.</returns>
        public virtual ApiDescriptionGroup GetOrAdd( ApiVersion apiVersion, Func<ApiVersion, string> formatName )
        {
            if ( formatName == null )
            {
                throw new ArgumentNullException( nameof( formatName ) );
            }

            if ( Count == 0 || !Dictionary.TryGetValue( apiVersion, out var group ) )
            {
                Add( group = new ApiDescriptionGroup( apiVersion ) { Name = formatName( apiVersion ) } );
            }

            return group;
        }

        /// <summary>
        /// Gets a read-only collection of all of the API versions in the collection.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
        public virtual IReadOnlyList<ApiVersion> ApiVersions
        {
            get
            {
                var keys = new List<ApiVersion>();

                for ( var i = 0; i < Count; i++ )
                {
                    keys.Add( GetKeyForItem( this[i] ) );
                }

                return keys.ToSortedReadOnlyList();
            }
        }

        /// <summary>
        /// Transforms all of the groups in the collection to a flat list of API descriptions.
        /// </summary>
        /// <returns>A flat, <see cref="Collection{T}">collection</see> of <see cref="ApiDescription">API descriptions</see>.</returns>
        public virtual Collection<ApiDescription> Flatten()
        {
            var flatApiDescriptions = new Collection<ApiDescription>();

            for ( var i = 0; i < ApiVersions.Count; i++ )
            {
                flatApiDescriptions.AddRange( this[ApiVersions[i]].ApiDescriptions );
            }

            return flatApiDescriptions;
        }
    }
}