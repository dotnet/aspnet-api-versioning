namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;

    sealed class ApiParameterDescriptionContext
    {
        internal ApiParameterDescriptionContext( ModelMetadata metadata, BindingInfo? bindingInfo, string? propertyName )
        {
            ModelMetadata = metadata;
            BinderModelName = BinderModelName = bindingInfo?.BinderModelName ?? metadata.BinderModelName;
            BindingSource = bindingInfo?.BindingSource ?? metadata.BindingSource;
            PropertyName = propertyName ?? metadata.PropertyName;
        }

        internal ModelMetadata ModelMetadata { get; set; }

        internal string BinderModelName { get; set; }

        internal BindingSource BindingSource { get; set; }

        internal string PropertyName { get; set; }
    }
}