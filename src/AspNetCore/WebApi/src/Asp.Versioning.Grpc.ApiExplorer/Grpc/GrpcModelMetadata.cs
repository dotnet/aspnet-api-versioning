// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

internal sealed class GrpcModelMetadata( ModelMetadataIdentity identity ) : ModelMetadata( identity )
{
    private string? dataTypeName;

    internal void SetDataTypeName( string value ) => dataTypeName = value;

    public override IReadOnlyDictionary<object, object> AdditionalValues { get; } =
        new Dictionary<object, object>( capacity: 0 );

    public override ModelPropertyCollection Properties { get; } = new( [] );

    public override string? BinderModelName { get; }

    public override Type? BinderType { get; }

    public override BindingSource? BindingSource { get; }

    public override bool ConvertEmptyStringToNull { get; }

    public override string? DataTypeName => dataTypeName;

    public override string? Description { get; }

    public override string? DisplayFormatString { get; }

    public override string? DisplayName { get; }

    public override string? EditFormatString { get; }

    public override ModelMetadata? ElementMetadata { get; }

    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues { get; }

    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues { get; }

    public override bool HasNonDefaultEditFormat { get; }

    public override bool HtmlEncode { get; }

    public override bool HideSurroundingHtml { get; }

    public override bool IsBindingAllowed => true;

    public override bool IsBindingRequired { get; }

    public override bool IsEnum { get; }

    public override bool IsFlagsEnum { get; }

    public override bool IsReadOnly { get; }

    public override bool IsRequired { get; }

    public override ModelBindingMessageProvider ModelBindingMessageProvider { get; } = default!;

    public override int Order { get; }

    public override string? Placeholder { get; }

    public override string? NullDisplayText { get; }

    public override IPropertyFilterProvider? PropertyFilterProvider { get; }

    public override bool ShowForDisplay { get; }

    public override bool ShowForEdit { get; }

    public override string? SimpleDisplayProperty { get; }

    public override string? TemplateHint { get; }

    public override bool ValidateChildren { get; }

    public override IReadOnlyList<object> ValidatorMetadata { get; } = [];

    public override Func<object, object?>? PropertyGetter { get; }

    public override Action<object, object?>? PropertySetter { get; }
}