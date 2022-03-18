// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

/// <summary>
/// Represents the original <see cref="IControllerNameConvention">controller name convention</see>.
/// </summary>
/// <remarks>This convention will apply the original convention which only strips the <b>Controller</b> suffix.</remarks>
public partial class OriginalControllerNameConvention : IControllerNameConvention
{
    /// <inheritdoc />
    public virtual string GroupName( string controllerName ) => controllerName;
}