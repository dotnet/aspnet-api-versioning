namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;

    /// <summary>
    /// Represents the API versioning configuration for ASP.NET Core <see cref="MvcOptions">MVC options</see>.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataMvcOptionsSetup : IPostConfigureOptions<MvcOptions>
    {
        /// <inheritdoc />
        public virtual void PostConfigure( string name, MvcOptions options )
        {
            var modelMetadataDetailsProviders = options.ModelMetadataDetailsProviders;

            ConfigureModelBoundType( modelMetadataDetailsProviders, typeof( ODataQueryOptions ), Special );
            ConfigureModelBoundType( modelMetadataDetailsProviders, typeof( ODataPath ), Special );
            ConfigureModelBoundType( modelMetadataDetailsProviders, typeof( IDelta ), Body );
            ConfigureModelBoundType( modelMetadataDetailsProviders, typeof( ODataActionParameters ), Body );
        }

        static void ConfigureModelBoundType(
            IList<IMetadataDetailsProvider> modelMetadataDetailsProviders,
            Type modelType,
            BindingSource bindingSource )
        {
            Contract.Requires( modelMetadataDetailsProviders != null );
            Contract.Requires( modelType != null );
            Contract.Requires( bindingSource != null );

            modelMetadataDetailsProviders.Insert( 0, new SuppressChildValidationMetadataProvider( modelType ) );
            modelMetadataDetailsProviders.Insert( 0, new BindingSourceMetadataProvider( modelType, bindingSource ) );
        }
    }
}