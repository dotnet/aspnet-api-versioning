// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Conventions;
using Asp.Versioning.OData;

/// <summary>
/// Represents the possible API versioning options for an OData API explorer.
/// </summary>
public partial class ODataApiExplorerOptions : ApiExplorerOptions
{
    private ODataQueryOptionsConventionBuilder? queryOptions;

    /// <summary>
    /// Gets or sets a value indicating whether qualified names are used when building URLs.
    /// </summary>
    /// <value>True if qualified names are used when building URLs; otherwise, false. The default value is <c>false</c>.</value>
    public bool UseQualifiedNames { get; set; }

    /// <summary>
    /// Gets or sets the default description used for OData related entity links.
    /// </summary>
    /// <value>The default description for OData related entity links. The default value
    /// is "The identifier of the related entity".</value>
    /// <remarks>OData related entity links appear in $ref requests. This description is used to describe dynamic parameters
    /// such as the $id query parameter.</remarks>
    public string RelatedEntityIdParameterDescription { get; set; } = ODataExpSR.RelatedEntityIdParamDesc;

    /// <summary>
    /// Gets or sets the convention builder used to describe OData query options.
    /// </summary>
    /// <value>An <see cref="ODataActionQueryOptionsConventionBuilder">OData query option convention builder</see>.</value>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    public ODataQueryOptionsConventionBuilder QueryOptions
    {
        get => queryOptions ??= new();
        set => queryOptions = value;
    }

    /// <summary>
    /// Gets or sets the OData metadata options used during API exploration.
    /// </summary>
    /// <value>One or more <see cref="ODataMetadataOptions"/> values.</value>
    public ODataMetadataOptions MetadataOptions { get; set; } = ODataMetadataOptions.None;

    /// <summary>
    /// Gets the builder used to create ad hoc versioned Entity Data Models (EDMs).
    /// </summary>
    /// <value>The associated <see cref="VersionedODataModelBuilder">model builder</see>.</value>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    public VersionedODataModelBuilder AdHocModelBuilder { get; }
}