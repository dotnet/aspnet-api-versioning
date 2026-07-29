// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

internal sealed class GrpcModelMetadata : ModelMetadata
{
    private readonly MessageDescriptor? messageDescriptor;
    private readonly ApiVersionMetadataCache? cache;
    private readonly ApiVersion? apiVersion;
    private readonly bool repeated;
    private string? dataTypeName;
    private ModelPropertyCollection? properties;
    private ModelMetadata? elementMetadata;

    public GrpcModelMetadata( ModelMetadataIdentity identity )
        : this( identity, default, default, default ) { }

    public GrpcModelMetadata( ModelMetadataIdentity identity, MessageDescriptor? messageDescriptor )
        : this( identity, messageDescriptor, default, default ) { }

    private GrpcModelMetadata(
        ModelMetadataIdentity identity,
        MessageDescriptor? messageDescriptor,
        ApiVersionMetadataCache? cache,
        ApiVersion? apiVersion,
        bool repeated = false )
        : base( identity )
    {
        this.messageDescriptor = messageDescriptor;
        this.cache = cache;
        this.apiVersion = apiVersion;
        this.repeated = repeated;
    }

    internal void SetDataTypeName( string value ) => dataTypeName = value;

    // the API version is only known after the versioned API Explorer has expanded the API description into one
    // result per version. the metadata of the original description is shared by every clone, so a new instance is
    // returned rather than mutating the existing one
    internal GrpcModelMetadata ForApiVersion( ApiVersionMetadataCache cache, ApiVersion apiVersion ) =>
        new( Identity, messageDescriptor, cache, apiVersion ) { dataTypeName = dataTypeName };

    public override IReadOnlyDictionary<object, object> AdditionalValues { get; } =
        new Dictionary<object, object>( capacity: 0 );

    // evaluated on demand so that a message which references itself, directly or transitively, doesn't recurse
    public override ModelPropertyCollection Properties => properties ??= NewProperties();

    public override string? BinderModelName { get; }

    public override Type? BinderType { get; }

    public override BindingSource? BindingSource { get; }

    public override bool ConvertEmptyStringToNull { get; }

    public override string? DataTypeName => dataTypeName;

    public override string? Description { get; }

    public override string? DisplayFormatString { get; }

    public override string? DisplayName { get; }

    public override string? EditFormatString { get; }

    // a repeated field is described by the schema of its element, so the message members hang off the element
    // rather than off the collection property itself
    public override ModelMetadata? ElementMetadata =>
        elementMetadata ??= repeated && messageDescriptor is not null && cache is not null && apiVersion is not null
            ? new GrpcModelMetadata( ModelMetadataIdentity.ForType( messageDescriptor.ClrType ), messageDescriptor, cache, apiVersion )
            : default;

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

    [UnconditionalSuppressMessage( "ILLink", "IL2075", Justification = "Message types are rooted by the generated gRPC service and are never trimmed" )]
    private ModelPropertyCollection NewProperties()
    {
        if ( messageDescriptor is null || cache is null || apiVersion is null || repeated )
        {
            return new( [] );
        }

        var fields = messageDescriptor.Fields.InDeclarationOrder();
        var members = new List<ModelMetadata>( fields.Count );

        for ( var i = 0; i < fields.Count; i++ )
        {
            var field = fields[i];

            if ( !cache.IsVisibleTo( field, apiVersion )
                || ModelType.GetProperty( field.PropertyName ) is not { } propertyInfo )
            {
                continue;
            }

            // a map is a repeated message under the covers, but its entry type is synthetic and cannot be annotated
            var nested = field.FieldType == FieldType.Message && !field.IsMap ? field.MessageType : default;
            var identity = ModelMetadataIdentity.ForProperty( propertyInfo, propertyInfo.PropertyType, ModelType );

            members.Add( new GrpcModelMetadata( identity, nested, cache, apiVersion, field.IsRepeated && !field.IsMap ) );
        }

        return new( members );
    }
}