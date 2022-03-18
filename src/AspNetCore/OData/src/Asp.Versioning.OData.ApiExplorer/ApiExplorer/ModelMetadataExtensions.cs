// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.CompilerServices;

internal static class ModelMetadataExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ModelMetadata SubstituteIfNecessary( this ModelMetadata modelMetadata, Type type )
    {
        if ( type.Equals( modelMetadata.ModelType ) )
        {
            return modelMetadata;
        }

        return new SubstitutedModelMetadata( modelMetadata, type );
    }
}