namespace Microsoft.Web.Http.Description
{
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Web.Http.Dispatcher;

    struct ClassProperty
    {
        readonly PropertyInfo clrProperty;
        internal readonly string Name;
        internal readonly Type Type;

        internal ClassProperty( PropertyInfo clrProperty )
        {
            Name = clrProperty.Name;
            Type = clrProperty.PropertyType;
            this.clrProperty = clrProperty;
        }

        internal ClassProperty( IAssembliesResolver assemblyResolver, IEdmOperationParameter parameter )
        {
            Name = parameter.Name;
            Type = parameter.Type.Definition.GetClrType( assemblyResolver );
            clrProperty = null;
        }

        internal IEnumerable<CustomAttributeBuilder> Attributes => GenerateAttributes();

        IEnumerable<CustomAttributeBuilder> GenerateAttributes()
        {
            if ( clrProperty == null )
            {
                yield break;
            }

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

        public override int GetHashCode() => ( Name.GetHashCode() * 397 ) ^ Type.GetHashCode();
    }
}