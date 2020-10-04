namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using System;
    using System.Collections.Generic;

#pragma warning disable CA1812

    sealed class SubstitutedModelMetadata : ModelMetadata
    {
        readonly ModelMetadata inner;

        public SubstitutedModelMetadata( ModelMetadata inner, Type substitutedModelType )
            : base( ModelMetadataIdentity.ForType( substitutedModelType ) ) => this.inner = inner;

        public override IReadOnlyDictionary<object, object> AdditionalValues => inner.AdditionalValues;

        public override ModelPropertyCollection Properties => inner.Properties;

        public override string BinderModelName => inner.BinderModelName;

        public override Type BinderType => inner.BinderType;

        public override BindingSource BindingSource => inner.BindingSource;

        public override bool ConvertEmptyStringToNull => inner.ConvertEmptyStringToNull;

        public override string DataTypeName => inner.DataTypeName;

        public override string Description => inner.Description;

        public override string DisplayFormatString => inner.DisplayFormatString;

#pragma warning disable CA1721 // Property names should not match get methods; inherited member
        public override string DisplayName => inner.DisplayName;
#pragma warning restore CA1721

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