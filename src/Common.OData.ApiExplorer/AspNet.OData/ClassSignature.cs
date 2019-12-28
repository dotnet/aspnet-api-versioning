namespace Microsoft.AspNet.OData
{
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    [DebuggerDisplay( "{Name,nq} ({ApiVersion,nq})" )]
    sealed class ClassSignature : IEquatable<ClassSignature>
    {
        static readonly ConstructorInfo newOriginalType = typeof( OriginalTypeAttribute ).GetConstructors()[0];
#if WEBAPI
        static readonly CustomAttributeBuilder[] NoAttributes = new CustomAttributeBuilder[0];
#else
        static readonly CustomAttributeBuilder[] NoAttributes = Array.Empty<CustomAttributeBuilder>();
#endif
        readonly Lazy<int> hashCode;

        internal ClassSignature( Type originalType, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            var attributeBuilders = new List<CustomAttributeBuilder>
            {
                new CustomAttributeBuilder( newOriginalType, new object[] { originalType } ),
            };

            attributeBuilders.AddRange( originalType.DeclaredAttributes() );

            Name = originalType.FullName!;
            Attributes = attributeBuilders.ToArray();
            Properties = properties.ToArray();
            ApiVersion = apiVersion;
            hashCode = new Lazy<int>( ComputeHashCode );
        }

        internal ClassSignature( string name, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
        {
            Name = name;
            Attributes = NoAttributes;
            Properties = properties.ToArray();
            ApiVersion = apiVersion;
            hashCode = new Lazy<int>( ComputeHashCode );
        }

        internal string Name { get; }

        internal IReadOnlyList<CustomAttributeBuilder> Attributes { get; }

        internal ClassProperty[] Properties { get; }

        internal ApiVersion ApiVersion { get; }

        public override int GetHashCode() => hashCode.Value;

        public override bool Equals( object? obj ) => obj is ClassSignature s && Equals( s );

        public bool Equals( ClassSignature other ) => GetHashCode() == other?.GetHashCode();

        int ComputeHashCode()
        {
            if ( Properties.Length == 0 )
            {
                return 0;
            }

            ref var property = ref Properties[0];
            var hash = property.GetHashCode();

            for ( var i = 1; i < Properties.Length; i++ )
            {
                property = ref Properties[i];
                hash = ( hash * 397 ) ^ Properties[i].GetHashCode();
            }

            return hash;
        }
    }
}