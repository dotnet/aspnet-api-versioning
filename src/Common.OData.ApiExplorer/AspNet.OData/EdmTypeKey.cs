namespace Microsoft.AspNet.OData
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
#endif
    using Microsoft.OData.Edm;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;

    struct EdmTypeKey : IEquatable<EdmTypeKey>
    {
        readonly int hashCode;

        internal EdmTypeKey( IEdmStructuredType type, ApiVersion apiVersion ) =>
            hashCode = ComputeHash( type.FullTypeName(), apiVersion );

        internal EdmTypeKey( IEdmTypeReference type, ApiVersion apiVersion ) =>
            hashCode = ComputeHash( type.FullName(), apiVersion );

        public static bool operator ==( EdmTypeKey obj, EdmTypeKey other ) => obj.Equals( other );

        public static bool operator !=( EdmTypeKey obj, EdmTypeKey other ) => !obj.Equals( other );

        public override int GetHashCode() => hashCode;

        public override bool Equals( object? obj ) => obj is EdmTypeKey other && Equals( other );

        public bool Equals( EdmTypeKey other ) => hashCode == other.hashCode;

#if WEBAPI
        static int ComputeHash( string fullName, ApiVersion apiVersion ) => ( fullName.GetHashCode() * 397 ) ^ apiVersion.GetHashCode();
#else
        static int ComputeHash( string fullName, ApiVersion apiVersion ) => HashCode.Combine( fullName, apiVersion );
#endif
    }
}