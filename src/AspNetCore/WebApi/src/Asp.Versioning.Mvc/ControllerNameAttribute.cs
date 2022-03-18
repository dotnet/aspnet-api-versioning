// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc.Routing;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public sealed partial class ControllerNameAttribute : RouteValueAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the controller.</param>
    public ControllerNameAttribute( string name ) : base( "controller", name ) => Name = name;
}