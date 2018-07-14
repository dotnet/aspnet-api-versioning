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
        readonly int hashCode;

        internal ClassSignature( string name, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            Contract.Requires( !string.IsNullOrEmpty( name ) );
            Contract.Requires( properties != null );
            Contract.Requires( apiVersion != null );

            Name = name;
            Properties = properties.ToArray();
            ApiVersion = apiVersion;

            if ( Properties.Count == 0 )
            {
                return;
            }

            hashCode = Properties[0].GetHashCode();

            for ( var i = 1; i < Properties.Count; i++ )
            {
                hashCode = ( hashCode * 397 ) ^ Properties[i].GetHashCode();
            }
        }

        internal string Name { get; }

        internal IReadOnlyList<ClassProperty> Properties { get; }

        internal ApiVersion ApiVersion { get; }

        public override int GetHashCode() => hashCode;

        public override bool Equals( object obj ) => obj is ClassSignature s && Equals( s );

        public bool Equals( ClassSignature other ) => hashCode == other?.hashCode;
    }
}