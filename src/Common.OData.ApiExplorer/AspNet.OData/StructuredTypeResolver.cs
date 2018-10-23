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
        readonly HashSet<Assembly> assemblies;

        internal StructuredTypeResolver( IEdmModel model, IEnumerable<Assembly> assemblies )
        {
            Contract.Requires( model != null );
            Contract.Requires( assemblies != null );

            this.model = model;
            this.assemblies = new HashSet<Assembly>( assemblies );
        }

        internal IEdmStructuredType GetStructuredType( Type type )
        {
            Contract.Requires( type != null );

            var structuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();
            var structuredType = structuredTypes.FirstOrDefault( t => type.Equals( t.GetClrType( assemblies ) ) );

            if ( structuredType == null && assemblies.Add( type.Assembly ) )
            {
                structuredType = structuredTypes.FirstOrDefault( t => type.Equals( t.GetClrType( assemblies ) ) );
            }

            return structuredType;
        }
    }
}