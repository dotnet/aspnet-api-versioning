namespace Microsoft.AspNet.OData
{
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    sealed class StructuredTypeResolver
    {
        readonly IEdmModel model;

        internal StructuredTypeResolver( IEdmModel model )
        {
            Contract.Requires( model != null );

            this.model = model;
        }

        internal IEdmStructuredType GetStructuredType( Type type )
        {
            Contract.Requires( type != null );

            var structuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();
            var structuredType = structuredTypes.FirstOrDefault( t => type.Equals( t.GetClrType( model ) ) );

            return structuredType;
        }
    }
}