namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the model metadata for an <see cref="ApiVersion">API version</see>.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiVersionModelMetadata : ModelMetadata
    {
        readonly ModelMetadata inner;
        readonly string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModelMetadata"/> class.
        /// </summary>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider">model metadata provider</see>
        /// used to create the new instance.</param>
        /// <param name="description">The description associated with the model metadata.</param>
        public ApiVersionModelMetadata( IModelMetadataProvider modelMetadataProvider, string description )
            : base( ModelMetadataIdentity.ForType( typeof( string ) ) )
        {
            if ( modelMetadataProvider == null )
            {
                throw new ArgumentNullException( nameof( modelMetadataProvider ) );
            }

            inner = modelMetadataProvider.GetMetadataForType( typeof( string ) );
            this.description = description;
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<object, object> AdditionalValues => inner.AdditionalValues;

        /// <inheritdoc />
        public override ModelPropertyCollection Properties => inner.Properties;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? BinderModelName => inner.BinderModelName;
#else
        public override string BinderModelName => inner.BinderModelName;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override Type? BinderType => inner.BinderType;
#else
        public override Type BinderType => inner.BinderType;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override BindingSource? BindingSource => inner.BindingSource;
#else
        public override BindingSource BindingSource => inner.BindingSource;
#endif

        /// <inheritdoc />
        public override bool ConvertEmptyStringToNull => inner.ConvertEmptyStringToNull;

        /// <inheritdoc />
        public override string DataTypeName => nameof( ApiVersion );

        /// <inheritdoc />
        public override string Description => description;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? DisplayFormatString => inner.DisplayFormatString;
#else
        public override string DisplayFormatString => inner.DisplayFormatString;
#endif

        /// <inheritdoc />
#pragma warning disable CA1721 // Property names should not match get methods; inherited member
        public override string DisplayName => SR.ApiVersionDisplayName;
#pragma warning restore CA1721

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? EditFormatString => inner.EditFormatString;
#else
        public override string EditFormatString => inner.EditFormatString;
#endif

        /// <inheritdoc />
        public override ModelMetadata? ElementMetadata => inner.ElementMetadata;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues => inner.EnumGroupedDisplayNamesAndValues;
#else
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues => inner.EnumGroupedDisplayNamesAndValues;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override IReadOnlyDictionary<string, string>? EnumNamesAndValues => inner.EnumNamesAndValues;
#else
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues => inner.EnumNamesAndValues;
#endif

        /// <inheritdoc />
        public override bool HasNonDefaultEditFormat => inner.HasNonDefaultEditFormat;

        /// <inheritdoc />
        public override bool HtmlEncode => inner.HtmlEncode;

        /// <inheritdoc />
        public override bool HideSurroundingHtml => inner.HideSurroundingHtml;

        /// <inheritdoc />
        public override bool IsBindingAllowed => inner.IsBindingAllowed;

        /// <inheritdoc />
        public override bool IsBindingRequired => inner.IsBindingRequired;

        /// <inheritdoc />
        public override bool IsEnum => inner.IsEnum;

        /// <inheritdoc />
        public override bool IsFlagsEnum => inner.IsFlagsEnum;

        /// <inheritdoc />
        public override bool IsReadOnly => inner.IsReadOnly;

        /// <inheritdoc />
        public override bool IsRequired => inner.IsRequired;

        /// <inheritdoc />
        public override ModelBindingMessageProvider ModelBindingMessageProvider => inner.ModelBindingMessageProvider;

        /// <inheritdoc />
        public override int Order => inner.Order;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? Placeholder => inner.Placeholder;
#else
        public override string Placeholder => inner.Placeholder;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? NullDisplayText => inner.NullDisplayText;
#else
        public override string NullDisplayText => inner.NullDisplayText;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override IPropertyFilterProvider? PropertyFilterProvider => inner.PropertyFilterProvider;
#else
        public override IPropertyFilterProvider PropertyFilterProvider => inner.PropertyFilterProvider;
#endif

        /// <inheritdoc />
        public override bool ShowForDisplay => inner.ShowForDisplay;

        /// <inheritdoc />
        public override bool ShowForEdit => inner.ShowForEdit;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? SimpleDisplayProperty => inner.SimpleDisplayProperty;
#else
        public override string SimpleDisplayProperty => inner.SimpleDisplayProperty;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override string? TemplateHint => inner.TemplateHint;
#else
        public override string TemplateHint => inner.TemplateHint;
#endif

        /// <inheritdoc />
        public override bool ValidateChildren => inner.ValidateChildren;

        /// <inheritdoc />
        public override IReadOnlyList<object> ValidatorMetadata => inner.ValidatorMetadata;

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override Func<object, object?>? PropertyGetter => inner.PropertyGetter;
#else
        public override Func<object, object> PropertyGetter => inner.PropertyGetter;
#endif

        /// <inheritdoc />
#if NET6_0_OR_GREATER
        public override Action<object, object?>? PropertySetter => inner.PropertySetter;
#else
        public override Action<object, object> PropertySetter => inner.PropertySetter;
#endif
    }
}