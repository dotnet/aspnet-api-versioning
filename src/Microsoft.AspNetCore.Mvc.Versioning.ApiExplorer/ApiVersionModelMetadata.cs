namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    sealed class ApiVersionModelMetadata : ModelMetadata
    {
        readonly ModelMetadata inner;
        readonly string description;

        internal ApiVersionModelMetadata( ModelMetadata inner, string description )
            : base( ModelMetadataIdentity.ForType( typeof( string ) ) )
        {
            Contract.Requires( inner != null );
            this.inner = inner;
            this.description = description;
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues => inner.AdditionalValues;

        public override ModelPropertyCollection Properties => inner.Properties;

        public override string BinderModelName => inner.BinderModelName;

        public override Type BinderType => inner.BinderType;

        public override BindingSource BindingSource => inner.BindingSource;

        public override bool ConvertEmptyStringToNull => inner.ConvertEmptyStringToNull;

        public override string DataTypeName => nameof( ApiVersion );

        public override string Description => description;

        public override string DisplayFormatString => inner.DisplayFormatString;

        public override string DisplayName => SR.ApiVersionDisplayName;

        public override string EditFormatString => inner.EditFormatString;

        public override ModelMetadata ElementMetadata => inner.ElementMetadata;

        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues => inner.EnumGroupedDisplayNamesAndValues;

        public override IReadOnlyDictionary<string, string> EnumNamesAndValues => inner.EnumNamesAndValues;

        public override bool HasNonDefaultEditFormat => inner.HasNonDefaultEditFormat;

        public override bool HtmlEncode => inner.HtmlEncode;

        public override bool HideSurroundingHtml => inner.HideSurroundingHtml;

        public override bool IsBindingAllowed => inner.IsBindingAllowed;

        public override bool IsBindingRequired => inner.IsBindingRequired;

        public override bool IsEnum => inner.IsEnum;

        public override bool IsFlagsEnum => inner.IsFlagsEnum;

        public override bool IsReadOnly => inner.IsReadOnly;

        public override bool IsRequired => inner.IsRequired;

        public override ModelBindingMessageProvider ModelBindingMessageProvider => inner.ModelBindingMessageProvider;

        public override int Order => inner.Order;

        public override string Placeholder => inner.Placeholder;

        public override string NullDisplayText => inner.NullDisplayText;

        public override IPropertyFilterProvider PropertyFilterProvider => inner.PropertyFilterProvider;

        public override bool ShowForDisplay => inner.ShowForDisplay;

        public override bool ShowForEdit => inner.ShowForEdit;

        public override string SimpleDisplayProperty => inner.SimpleDisplayProperty;

        public override string TemplateHint => inner.TemplateHint;

        public override bool ValidateChildren => inner.ValidateChildren;

        public override IReadOnlyList<object> ValidatorMetadata => inner.ValidatorMetadata;

        public override Func<object, object> PropertyGetter => inner.PropertyGetter;

        public override Action<object, object> PropertySetter => inner.PropertySetter;
    }
}