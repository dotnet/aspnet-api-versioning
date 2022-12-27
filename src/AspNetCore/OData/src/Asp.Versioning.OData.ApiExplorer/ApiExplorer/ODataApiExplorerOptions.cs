// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.OData;
using Microsoft.Extensions.Options;
using System.ComponentModel;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
public partial class ODataApiExplorerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
    /// </summary>
    /// <remarks>This constructor is only intended to satisfy the parameterless constructor
    /// requirement on the <see cref="IOptionsFactory{T}"/> type constraint.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ODataApiExplorerOptions() =>
        AdHocModelBuilder = new( ODataApiVersionCollectionProvider.Empty, Enumerable.Empty<IModelConfiguration>() );

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
    /// </summary>
    /// <param name="modelBuilder">The associated <see cref="VersionedODataModelBuilder">model builder</see>.</param>
    [CLSCompliant( false )]
    public ODataApiExplorerOptions( VersionedODataModelBuilder modelBuilder ) =>
        AdHocModelBuilder = modelBuilder ?? throw new ArgumentNullException( nameof( modelBuilder ) );

    private sealed class ODataApiVersionCollectionProvider : IODataApiVersionCollectionProvider
    {
        private ODataApiVersionCollectionProvider() { }

        internal static ODataApiVersionCollectionProvider Empty { get; } = new();

        public IReadOnlyList<ApiVersion> ApiVersions { get; set; } = Array.Empty<ApiVersion>();
    }
}