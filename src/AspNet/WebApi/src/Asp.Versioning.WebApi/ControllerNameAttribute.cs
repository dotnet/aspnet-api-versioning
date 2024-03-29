﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public sealed partial class ControllerNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the controller.</param>
    public ControllerNameAttribute( string name ) => Name = name;
}