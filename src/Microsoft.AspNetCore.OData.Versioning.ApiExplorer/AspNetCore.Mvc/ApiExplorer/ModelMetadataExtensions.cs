namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;

    internal static class ModelMetadataExtensions
    {
        internal static ModelMetadata SubstituteIfNecessary( this ModelMetadata modelMetadata, Type type )
        {
            if ( type.Equals( modelMetadata.ModelType ) )
            {
                return modelMetadata;
            }

            return new SubstitutedModelMetadata( modelMetadata, type );
        }
    }
}