// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;
using System.Reflection;

internal sealed class StructuredTypeResolver
{
    private readonly IEdmModel? model;

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