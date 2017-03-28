namespace Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.Extensions.Localization;

    class TestModelMetadataProvider
    {
        internal static IModelMetadataProvider CreateDefaultProvider( IStringLocalizerFactory stringLocalizerFactory = null )
        {
            var detailsProviders = new IMetadataDetailsProvider[]
            {
                new DefaultBindingMetadataProvider(),
                new DefaultValidationMetadataProvider(),
                new DataAnnotationsMetadataProvider( new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(), stringLocalizerFactory ),
                new DataMemberRequiredBindingMetadataProvider(),
            };
            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider( detailsProviders );

            return new DefaultModelMetadataProvider( compositeDetailsProvider, new TestOptionsManager<MvcOptions>() );
        }
    }
}