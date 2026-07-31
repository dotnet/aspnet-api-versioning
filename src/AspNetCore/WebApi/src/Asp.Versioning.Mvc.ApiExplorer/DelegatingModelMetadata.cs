// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

/// <summary>
/// Represents <see cref="ModelMetadata">model metadata</see> that delegates to other model metadata.
/// </summary>
/// <remarks>
/// <see cref="ModelMetadata"/> declares three dozen abstract members, which makes decorating it verbose. This
/// class forwards every member to the <see cref="Inner">inner</see> metadata so that a derived class only has to
/// override the members it actually changes.
/// </remarks>
[CLSCompliant( false )]
public abstract class DelegatingModelMetadata : ModelMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingModelMetadata"/> class.
    /// </summary>
    /// <param name="inner">The <see cref="ModelMetadata">model metadata</see> to delegate to.</param>
    /// <param name="identity">The <see cref="ModelMetadataIdentity">identity</see> of the model metadata.</param>
    protected DelegatingModelMetadata( ModelMetadata inner, ModelMetadataIdentity identity )
        : base( identity )
    {
        ArgumentNullException.ThrowIfNull( inner );
        Inner = inner;
    }

    /// <summary>
    /// Gets the model metadata all members are delegated to.
    /// </summary>
    /// <value>The inner <see cref="ModelMetadata">model metadata</see>.</value>
    protected ModelMetadata Inner { get; }

    /// <inheritdoc />
    public override IReadOnlyDictionary<object, object> AdditionalValues => Inner.AdditionalValues;

    /// <inheritdoc />
    public override ModelPropertyCollection Properties => Inner.Properties;

    /// <inheritdoc />
    public override string? BinderModelName => Inner.BinderModelName;

    /// <inheritdoc />
    public override Type? BinderType => Inner.BinderType;

    /// <inheritdoc />
    public override BindingSource? BindingSource => Inner.BindingSource;

    /// <inheritdoc />
    public override bool ConvertEmptyStringToNull => Inner.ConvertEmptyStringToNull;

    /// <inheritdoc />
    public override string? DataTypeName => Inner.DataTypeName;

    /// <inheritdoc />
    public override string? Description => Inner.Description;

    /// <inheritdoc />
    public override string? DisplayFormatString => Inner.DisplayFormatString;

    /// <inheritdoc />
    public override string? DisplayName => Inner.DisplayName;

    /// <inheritdoc />
    public override string? EditFormatString => Inner.EditFormatString;

    /// <inheritdoc />
    public override ModelMetadata? ElementMetadata => Inner.ElementMetadata;

    /// <inheritdoc />
    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues =>
        Inner.EnumGroupedDisplayNamesAndValues;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues => Inner.EnumNamesAndValues;

    /// <inheritdoc />
    public override bool HasNonDefaultEditFormat => Inner.HasNonDefaultEditFormat;

    /// <inheritdoc />
    public override bool HtmlEncode => Inner.HtmlEncode;

    /// <inheritdoc />
    public override bool HideSurroundingHtml => Inner.HideSurroundingHtml;

    /// <inheritdoc />
    public override bool IsBindingAllowed => Inner.IsBindingAllowed;

    /// <inheritdoc />
    public override bool IsBindingRequired => Inner.IsBindingRequired;

    /// <inheritdoc />
    public override bool IsEnum => Inner.IsEnum;

    /// <inheritdoc />
    public override bool IsFlagsEnum => Inner.IsFlagsEnum;

    /// <inheritdoc />
    public override bool IsReadOnly => Inner.IsReadOnly;

    /// <inheritdoc />
    public override bool IsRequired => Inner.IsRequired;

    /// <inheritdoc />
    public override ModelBindingMessageProvider ModelBindingMessageProvider => Inner.ModelBindingMessageProvider;

    /// <inheritdoc />
    public override int Order => Inner.Order;

    /// <inheritdoc />
    public override string? Placeholder => Inner.Placeholder;

    /// <inheritdoc />
    public override string? NullDisplayText => Inner.NullDisplayText;

    /// <inheritdoc />
    public override IPropertyFilterProvider? PropertyFilterProvider => Inner.PropertyFilterProvider;

    /// <inheritdoc />
    public override bool ShowForDisplay => Inner.ShowForDisplay;

    /// <inheritdoc />
    public override bool ShowForEdit => Inner.ShowForEdit;

    /// <inheritdoc />
    public override string? SimpleDisplayProperty => Inner.SimpleDisplayProperty;

    /// <inheritdoc />
    public override string? TemplateHint => Inner.TemplateHint;

    /// <inheritdoc />
    public override bool ValidateChildren => Inner.ValidateChildren;

    /// <inheritdoc />
    public override IReadOnlyList<object> ValidatorMetadata => Inner.ValidatorMetadata;

    /// <inheritdoc />
    public override Func<object, object?>? PropertyGetter => Inner.PropertyGetter;

    /// <inheritdoc />
    public override Action<object, object?>? PropertySetter => Inner.PropertySetter;
}