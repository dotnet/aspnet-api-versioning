namespace Microsoft.AspNet.OData
{
    using Microsoft.OData.Edm;
    using System;
    using System.Linq;
    using System.Reflection;

    sealed class StructuredTypeResolver
    {
        readonly IEdmModel? model;

        internal StructuredTypeResolver( IEdmModel? model ) => this.model = model;

        internal IEdmStructuredType? GetStructuredType( Type type )
        {
            if ( model == null )
            {
                return default;
            }

            var structuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();
            var structuredType = structuredTypes.FirstOrDefault( t => type.Equals( t.GetClrType( model ) ) );

            if ( structuredType == null )
            {
                var original = type.GetCustomAttribute<OriginalTypeAttribute>( inherit: false );

                if ( original != null )
                {
                    return GetStructuredType( original.Type );
                }
            }

            return structuredType;
        }
    }
}