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
    using System.Reflection;
    using System.Reflection.Emit;

    sealed class ClassSignature : IEquatable<ClassSignature>
    {
#if WEBAPI
        static readonly CustomAttributeBuilder[] NoAttributes = new CustomAttributeBuilder[0];
#else
        static readonly CustomAttributeBuilder[] NoAttributes = Array.Empty<CustomAttributeBuilder>();
#endif
        static readonly ConstructorInfo newOriginalType = typeof( OriginalTypeAttribute ).GetConstructors()[0];
        readonly Lazy<int> hashCode;

        internal ClassSignature( Type originalType, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            Contract.Requires( originalType != null );
            Contract.Requires( properties != null );
            Contract.Requires( apiVersion != null );

            var attributeBuilders = new List<CustomAttributeBuilder>( originalType.DeclaredAttributes() );

            attributeBuilders.Insert( 0, new CustomAttributeBuilder( newOriginalType, new object[] { originalType } ) );
            Name = originalType.FullName;
            Attributes = attributeBuilders.ToArray();
            Properties = properties.ToArray();
            ApiVersion = apiVersion;
            hashCode = new Lazy<int>( ComputeHashCode );
        }

        internal ClassSignature( string name, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            Contract.Requires( !string.IsNullOrEmpty( name ) );
            Contract.Requires( properties != null );
            Contract.Requires( apiVersion != null );

            Name = name;
            Attributes = NoAttributes;
            Properties = properties.ToArray();
            ApiVersion = apiVersion;
            hashCode = new Lazy<int>( ComputeHashCode );
        }

        internal string Name { get; }

        internal IEnumerable<CustomAttributeBuilder> Attributes { get; }

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