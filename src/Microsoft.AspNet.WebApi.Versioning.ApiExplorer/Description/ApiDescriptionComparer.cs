namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Description;
    using static System.StringComparison;

    sealed class ApiDescriptionComparer : IEqualityComparer<ApiDescription>, IEqualityComparer<VersionedApiDescription>, IComparer<VersionedApiDescription>
    {
        readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        internal static ApiDescriptionComparer Instance { get; } = new ApiDescriptionComparer();

        public bool Equals( VersionedApiDescription x, VersionedApiDescription y )
        {
            if ( x == null )
            {
                return y == null;
            }
            else if ( y == null )
            {
                return false;
            }

            if ( string.Equals( x.UniqueID, y.UniqueID, OrdinalIgnoreCase ) )
            {
                return x.Version == y.Version;
            }

            return false;
        }

        public int GetHashCode( VersionedApiDescription obj )
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
            var apiVersion = obj.Version;

            return ( hash * 397 ) ^ apiVersion?.GetHashCode() ?? 0;
        }

        public bool Equals( ApiDescription x, ApiDescription y )
        {
            var id1 = default( string );
            var id2 = default( string );

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

                id1 = x1.UniqueID;
                id2 = y.ID;
            }
            else if ( y is VersionedApiDescription y1 )
            {
                id1 = x.ID;
                id2 = y1.UniqueID;
            }
            else
            {
                id1 = x.ID;
                id2 = y.ID;
            }

            return string.Equals( id1, id2, OrdinalIgnoreCase );
        }

        public int GetHashCode( ApiDescription obj )
        {
            if ( obj is VersionedApiDescription other )
            {
                return GetHashCode( other );
            }

            var id = obj.ID;

            return id == null ? 0 : comparer.GetHashCode( id );
        }

        public int Compare( VersionedApiDescription x, VersionedApiDescription y )
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
                    result = CompareVersions( x.Version, y.Version );
                }
            }

            return result;
        }

        int CompareStrings( string string1, string string2 )
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