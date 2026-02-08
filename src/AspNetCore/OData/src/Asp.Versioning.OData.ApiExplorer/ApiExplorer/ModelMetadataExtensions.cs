// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;

internal static class ModelMetadataExtensions
{
    extension( ModelMetadata modelMetadata )
    {
        internal ModelMetadata SubstituteIfNecessary( Type type )
        {
            if ( type.Equals( modelMetadata.ModelType ) )
            {
                return modelMetadata;
            }

            return new SubstitutedModelMetadata( modelMetadata, type );
        }
    }
}