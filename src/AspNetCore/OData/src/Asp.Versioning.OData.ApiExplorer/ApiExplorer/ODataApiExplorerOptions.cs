// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.OData;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
public partial class ODataApiExplorerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
    /// </summary>
    /// <param name="modelBuilder">The associated <see cref="VersionedODataModelBuilder">model builder</see>.</param>
    [CLSCompliant( false )]
    public ODataApiExplorerOptions( VersionedODataModelBuilder modelBuilder ) =>
        AdHocModelBuilder = modelBuilder ?? throw new ArgumentNullException( nameof( modelBuilder ) );
}