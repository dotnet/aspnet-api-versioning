// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

/// <content>
/// Provides additional implementation specific ASP.NET Core.
/// </content>
public partial class OriginalControllerNameConvention
{
    /// <inheritdoc />
    public virtual string NormalizeName( string controllerName ) => controllerName; // already normalized
}