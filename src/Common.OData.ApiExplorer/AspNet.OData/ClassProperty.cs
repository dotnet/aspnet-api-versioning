namespace Microsoft.AspNet.OData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;

    struct ClassProperty
    {
        internal readonly Type Type;
        internal readonly string Name;

        internal ClassProperty( PropertyInfo clrProperty, Type propertyType )
        {
            Contract.Requires( clrProperty != null );
            Contract.Requires( propertyType != null );

            Name = clrProperty.Name;
            Type = propertyType;
            Attributes = clrProperty.DeclaredAttributes();
        }

        internal ClassProperty( IServiceProvider services, IEdmOperationParameter parameter, IModelTypeBuilder typeBuilder )
        {
            Contract.Requires( services != null );
            Contract.Requires( parameter != null );
            Contract.Requires( typeBuilder != null );

            Name = parameter.Name;
            var context = new TypeSubstitutionContext( services, typeBuilder );

            if ( parameter.Type.IsCollection() )
            {
                var collectionType = parameter.Type.AsCollection();
                var elementType = collectionType.ElementType().Definition.GetClrType( services.GetRequiredService<IEdmModel>() );
                var substitutedType = elementType.SubstituteIfNecessary( context );

                Type = typeof( IEnumerable<> ).MakeGenericType( substitutedType );
            }
            else
            {
                var parameterType = parameter.Type.Definition.GetClrType( services.GetRequiredService<IEdmModel>() );

                Type = parameterType.SubstituteIfNecessary( context );
            }

            Attributes = AttributesFromOperationParameter( parameter );
        }

        internal IEnumerable<CustomAttributeBuilder> Attributes { get; }

        public override int GetHashCode() => ( Name.GetHashCode() * 397 ) ^ Type.GetHashCode();

        static IEnumerable<CustomAttributeBuilder> AttributesFromOperationParameter( IEdmOperationParameter parameter )
        {
            Contract.Requires( parameter != null );
            Contract.Ensures( Contract.Result<IEnumerable<CustomAttributeBuilder>>() != null );

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