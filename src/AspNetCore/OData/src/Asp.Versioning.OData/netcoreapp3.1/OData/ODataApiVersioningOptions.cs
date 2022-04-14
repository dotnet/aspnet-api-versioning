// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.Extensions.Options;
using System.ComponentModel;

/// <content>
/// Additional implementation specific ASP.NET Core 3.1.
/// </content>
public partial class ODataApiVersioningOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiVersioningOptions"/> class.
    /// </summary>
    /// <remarks>This constructor is meant to serve the public parameter type constraint of
    /// <see cref="IOptionsFactory{T}"/>, but should not be used.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ODataApiVersioningOptions() :
        this( new( new ODataApiVersionCollectionProvider(), Enumerable.Empty<IModelConfiguration>() ) )
    {
    }
}