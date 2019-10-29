namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Description;
    using static System.StringComparison;

    /// <summary>
    /// Represents an object that compares <see cref="ApiDescription">API Descriptions</see><seealso cref="VersionedApiDescription"/>.
    /// </summary>
    public class ApiDescriptionComparer :
        IEqualityComparer<ApiDescription>,
        IEqualityComparer<VersionedApiDescription>,
        IComparer<ApiDescription>,
        IComparer<VersionedApiDescription>
    {
        readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Determines whether the two <see cref="VersionedApiDescription">API descriptions</see> are equal.
        /// </summary>
        /// <param name="x">The <see cref="VersionedApiDescription">API descriptions</see> to compare.</param>
        /// <param name="y">The <see cref="VersionedApiDescription">API descriptions</see> to compare against.</param>
        /// <returns>True if the two API descriptions are equal; otherwise, false.</returns>
        public virtual bool Equals( VersionedApiDescription x, VersionedApiDescription y )
        {
            if ( x == null )
            {
                return y == null;
            }
            else if ( y == null )
            {
                return false;
            }

            if ( string.Equals( x.ID, y.ID, OrdinalIgnoreCase ) )
            {
                return x.ApiVersion == y.ApiVersion;
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="VersionedApiDescription">API description</see>.
        /// </summary>
        /// <param name="obj">The object to get a hash code for.</param>
        /// <returns>The hash code of the specified object.</returns>
        public virtual int GetHashCode( VersionedApiDescription obj )
        {
            if ( obj == null )
            {
                return 0;
            }

            var id = obj.ID;

            if ( id == null )
            {
                return 0;
            }

            var hash = comparer.GetHashCode( id );
            var apiVersion = obj.ApiVersion;

            if ( apiVersion != null )
            {
                hash = ( hash * 397 ) ^ apiVersion.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Determines whether the two <see cref="ApiDescription">API descriptions</see> are equal.
        /// </summary>
        /// <param name="x">The <see cref="ApiDescription">API descriptions</see> to compare.</param>
        /// <param name="y">The <see cref="ApiDescription">API descriptions</see> to compare against.</param>
        /// <returns>True if the two API descriptions are equal; otherwise, false.</returns>
        public virtual bool Equals( ApiDescription x, ApiDescription y )
        {
            string id1;
            string id2;

            if ( x == null )
            {
                return y == null;
            }
            else if ( y == null )
            {
                return false;
            }
            else if ( x is VersionedApiDescription x1 )
            {
                if ( y is VersionedApiDescription y1 )
                {
                    return Equals( x1, y1 );
                }

                id1 = x1.GetUniqueID();
                id2 = y.ID;
            }
            else if ( y is VersionedApiDescription y1 )
            {
                id1 = x.ID;
                id2 = y1.GetUniqueID();
            }
            else
            {
                id1 = x.ID;
                id2 = y.ID;
            }

            return string.Equals( id1, id2, OrdinalIgnoreCase );
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="ApiDescription">API description</see>.
        /// </summary>
        /// <param name="obj">The object to get a hash code for.</param>
        /// <returns>The hash code of the specified object.</returns>
        public virtual int GetHashCode( ApiDescription obj )
        {
            if ( obj is VersionedApiDescription other )
            {
                return GetHashCode( other );
            }

            var id = obj?.ID;

            return id == null ? 0 : comparer.GetHashCode( id );
        }

        /// <summary>
        /// Compares two <see cref="VersionedApiDescription">API descriptions</see>.
        /// </summary>
        /// <param name="x">The <see cref="VersionedApiDescription">API descriptions</see> to compare.</param>
        /// <param name="y">The <see cref="VersionedApiDescription">API descriptions</see> to compare against.</param>
        /// <returns>0 if the objects are equal, 1 if <paramref name="x"/> is greater than <paramref name="y"/>,
        /// or -1 if <paramref name="x"/> is less than <paramref name="y"/>.</returns>
        public virtual int Compare( VersionedApiDescription x, VersionedApiDescription y )
        {
            if ( x == null )
            {
                return y == null ? 0 : -1;
            }
            else if ( y == null )
            {
                return 1;
            }

            var result = CompareStrings( x.HttpMethod?.Method, y.HttpMethod?.Method );

            if ( result == 0 )
            {
                result = CompareStrings( x.RelativePath, y.RelativePath );

                if ( result == 0 )
                {
                    result = CompareVersions( x.ApiVersion, y.ApiVersion );
                }
            }

            return result;
        }

        /// <summary>
        /// Compares two <see cref="ApiDescription">API descriptions</see>.
        /// </summary>
        /// <param name="x">The <see cref="ApiDescription">API descriptions</see> to compare.</param>
        /// <param name="y">The <see cref="ApiDescription">API descriptions</see> to compare against.</param>
        /// <returns>0 if the objects are equal, 1 if <paramref name="x"/> is greater than <paramref name="y"/>,
        /// or -1 if <paramref name="x"/> is less than <paramref name="y"/>.</returns>
        public virtual int Compare( ApiDescription x, ApiDescription y )
        {
            if ( x == null )
            {
                return y == null ? 0 : -1;
            }
            else if ( y == null )
            {
                return 1;
            }

            var result = CompareStrings( x.HttpMethod?.Method, y.HttpMethod?.Method );

            if ( result == 0 )
            {
                result = CompareStrings( x.RelativePath, y.RelativePath );
            }

            return result;
        }

        int CompareStrings( string? string1, string? string2 )
        {
            if ( string1 == null )
            {
                return string2 == null ? 0 : -1;
            }
            else if ( string2 == null )
            {
                return 1;
            }

            var len1 = string1.Length;
            var len2 = string2.Length;

            if ( len1 > len2 )
            {
                return 1;
            }
            else if ( len2 > len1 )
            {
                return -1;
            }

            return comparer.Compare( string1, string2 );
        }

        static int CompareVersions( ApiVersion version1, ApiVersion version2 )
        {
            if ( version1 == null )
            {
                return version2 == null ? 0 : -1;
            }
            else if ( version2 == null )
            {
                return 1;
            }

            return version1.CompareTo( version2 );
        }
    }
}