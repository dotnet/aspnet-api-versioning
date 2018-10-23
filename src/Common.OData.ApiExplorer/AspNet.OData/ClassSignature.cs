namespace Microsoft.AspNet.OData
{
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    sealed class ClassSignature : IEquatable<ClassSignature>
    {
        readonly Lazy<int> hashCode;

        internal ClassSignature( string name, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            Contract.Requires( !string.IsNullOrEmpty( name ) );
            Contract.Requires( properties != null );
            Contract.Requires( apiVersion != null );

            Name = name;
            Properties = properties.ToArray();
            ApiVersion = apiVersion;
            hashCode = new Lazy<int>( ComputeHashCode );
        }

        internal string Name { get; }

        internal IReadOnlyList<ClassProperty> Properties { get; }

        internal ApiVersion ApiVersion { get; }

        public override int GetHashCode() => hashCode.Value;

        public override bool Equals( object obj ) => obj is ClassSignature s && Equals( s );

        public bool Equals( ClassSignature other ) => GetHashCode() == other?.GetHashCode();

        int ComputeHashCode()
        {
            if ( Properties.Count == 0 )
            {
                return 0;
            }

            var hash = Properties[0].GetHashCode();

            for ( var i = 1; i < Properties.Count; i++ )
            {
                hash = ( hash * 397 ) ^ Properties[i].GetHashCode();
            }

            return hash;
        }
    }
}