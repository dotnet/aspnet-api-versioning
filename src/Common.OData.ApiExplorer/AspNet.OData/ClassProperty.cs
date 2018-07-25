﻿namespace Microsoft.AspNet.OData
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif

    struct ClassProperty
    {
        internal readonly string Name;
        internal readonly Type Type;

        internal ClassProperty( PropertyInfo clrProperty )
        {
            Contract.Requires( clrProperty != null );

            Name = clrProperty.Name;
            Type = clrProperty.PropertyType;
            Attributes = AttributesFromProperty( clrProperty );
        }

        internal ClassProperty( IEnumerable<Assembly> assemblies, IEdmOperationParameter parameter )
        {
            Contract.Requires( assemblies != null );
            Contract.Requires( parameter != null );

            Name = parameter.Name;
            Type = parameter.Type.Definition.GetClrType( assemblies );
            Attributes = AttributesFromOperationParameter( parameter );
        }

        internal IEnumerable<CustomAttributeBuilder> Attributes { get; }

        static IEnumerable<CustomAttributeBuilder> AttributesFromProperty( PropertyInfo clrProperty )
        {
            Contract.Requires( clrProperty != null );
            Contract.Ensures( Contract.Result<IEnumerable<CustomAttributeBuilder>>() != null );

            foreach ( var attribute in clrProperty.CustomAttributes )
            {
                var ctor = attribute.Constructor;
                var ctorArgs = attribute.ConstructorArguments.Select( a => a.Value ).ToArray();
                var namedProperties = new List<PropertyInfo>( attribute.NamedArguments.Count );
                var propertyValues = new List<object>( attribute.NamedArguments.Count );
                var namedFields = new List<FieldInfo>( attribute.NamedArguments.Count );
                var fieldValues = new List<object>( attribute.NamedArguments.Count );

                foreach ( var argument in attribute.NamedArguments )
                {
                    if ( argument.IsField )
                    {
                        namedFields.Add( (FieldInfo) argument.MemberInfo );
                        fieldValues.Add( argument.TypedValue.Value );
                    }
                    else
                    {
                        namedProperties.Add( (PropertyInfo) argument.MemberInfo );
                        propertyValues.Add( argument.TypedValue.Value );
                    }
                }

                yield return new CustomAttributeBuilder(
                    ctor,
                    ctorArgs,
                    namedProperties.ToArray(),
                    propertyValues.ToArray(),
                    namedFields.ToArray(),
                    fieldValues.ToArray() );
            }
        }

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

        public override int GetHashCode() => ( Name.GetHashCode() * 397 ) ^ Type.GetHashCode();
    }
}