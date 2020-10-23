namespace Microsoft.AspNet.OData
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    [DebuggerDisplay( "{Name,nq}" )]
    readonly struct ClassProperty
    {
        internal readonly Type Type;
        internal readonly string Name;

        internal ClassProperty( PropertyInfo clrProperty, Type propertyType )
        {
            Name = clrProperty.Name;
            Type = propertyType;
            Attributes = clrProperty.DeclaredAttributes().ToArray();
        }

        internal ClassProperty( IEdmOperationParameter parameter, TypeSubstitutionContext context )
        {
            Name = parameter.Name;

            if ( parameter.Type.IsCollection() )
            {
                var collectionType = parameter.Type.AsCollection();
                var elementType = collectionType.ElementType().Definition.GetClrType( context.Model )!;
                var substitutedType = elementType.SubstituteIfNecessary( context );

                Type = typeof( IEnumerable<> ).MakeGenericType( substitutedType );
            }
            else
            {
                var parameterType = parameter.Type.Definition.GetClrType( context.Model )!;

                Type = parameterType.SubstituteIfNecessary( context );
            }

            Attributes = AttributesFromOperationParameter( parameter ).ToArray();
        }

        internal IReadOnlyList<CustomAttributeBuilder> Attributes { get; }

#if WEBAPI
        public override int GetHashCode() => ( Name.GetHashCode() * 397 ) ^ Type.GetHashCode();
#else
        public override int GetHashCode() => HashCode.Combine( Name, Type );
#endif
        static IEnumerable<CustomAttributeBuilder> AttributesFromOperationParameter( IEdmOperationParameter parameter )
        {
            if ( parameter.Type.IsNullable )
            {
                yield break;
            }

            var ctor = typeof( RequiredAttribute ).GetConstructors().Where( c => c.GetParameters().Length == 0 ).Single();
#if WEBAPI
            var args = new object[0];
#else
            var args = Array.Empty<object>();
#endif

            yield return new CustomAttributeBuilder( ctor, args );
        }
    }
}