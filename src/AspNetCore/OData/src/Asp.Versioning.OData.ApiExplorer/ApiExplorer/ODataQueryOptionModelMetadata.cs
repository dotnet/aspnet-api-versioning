// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

/// <summary>
/// Represents the model metadata for an OData query option.
/// </summary>
[CLSCompliant( false )]
public sealed class ODataQueryOptionModelMetadata : ModelMetadata
{
    private readonly ModelMetadata inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataQueryOptionModelMetadata"/> class.
    /// </summary>
    /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider">model metadata provider</see>
    /// used to create the new instance.</param>
    /// <param name="modelType">The type of OData query option.</param>
    /// <param name="description">The description associated with the model metadata.</param>
    public ODataQueryOptionModelMetadata( IModelMetadataProvider modelMetadataProvider, Type modelType, string description )
        : base( ModelMetadataIdentity.ForType( modelType ) )
    {
        ArgumentNullException.ThrowIfNull( modelMetadataProvider );

        inner = modelMetadataProvider.GetMetadataForType( modelType );
        Description = description;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<object, object> AdditionalValues => inner.AdditionalValues;

    /// <inheritdoc />
    public override ModelPropertyCollection Properties => inner.Properties;

    /// <inheritdoc />
    public override string? BinderModelName => inner.BinderModelName;

    /// <inheritdoc />
    public override Type? BinderType => inner.BinderType;

    /// <inheritdoc />
    public override BindingSource? BindingSource => inner.BindingSource;

    /// <inheritdoc />
    public override bool ConvertEmptyStringToNull => inner.ConvertEmptyStringToNull;

    /// <inheritdoc />
    public override string? DataTypeName => inner.DataTypeName;

    /// <inheritdoc />
    public override string Description { get; }

    /// <inheritdoc />
    public override string? DisplayFormatString => inner.DisplayFormatString;

    /// <inheritdoc />
    public override string? DisplayName => inner.DisplayName;

    /// <inheritdoc />
    public override string? EditFormatString => inner.EditFormatString;

    /// <inheritdoc />
    public override ModelMetadata? ElementMetadata => inner.ElementMetadata;

    /// <inheritdoc />
    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues => inner.EnumGroupedDisplayNamesAndValues;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues => inner.EnumNamesAndValues;

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
    public override string? Placeholder => inner.Placeholder;

    /// <inheritdoc />
    public override string? NullDisplayText => inner.NullDisplayText;

    /// <inheritdoc />
    public override IPropertyFilterProvider? PropertyFilterProvider => inner.PropertyFilterProvider;

    /// <inheritdoc />
    public override bool ShowForDisplay => inner.ShowForDisplay;

    /// <inheritdoc />
    public override bool ShowForEdit => inner.ShowForEdit;

    /// <inheritdoc />
    public override string? SimpleDisplayProperty => inner.SimpleDisplayProperty;

    /// <inheritdoc />
    public override string? TemplateHint => inner.TemplateHint;

    /// <inheritdoc />
    public override bool ValidateChildren => inner.ValidateChildren;

    /// <inheritdoc />
    public override IReadOnlyList<object> ValidatorMetadata => inner.ValidatorMetadata;

    /// <inheritdoc />
    public override Func<object, object?>? PropertyGetter => inner.PropertyGetter;

    /// <inheritdoc />
    public override Action<object, object?>? PropertySetter => inner.PropertySetter;
}