namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;

    sealed class ApiParameterDescriptionContext
    {
        public ApiParameterDescriptionContext( ModelMetadata metadata, BindingInfo bindingInfo, string propertyName )
        {
            ModelMetadata = metadata;
            BinderModelName = bindingInfo?.BinderModelName ?? metadata.BinderModelName;
            BindingSource = bindingInfo?.BindingSource ?? metadata.BindingSource;
            PropertyName = propertyName ?? metadata.PropertyName;
        }

        public ModelMetadata ModelMetadata { get; set; }

        public string BinderModelName { get; set; }

        public BindingSource BindingSource { get; set; }

        public string PropertyName { get; set; }
    }
}