// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents a callback function used to format a group name.
/// </summary>
/// <param name="groupName">The associated group name.</param>
/// <param name="apiVersion">A formatted API version.</param>
/// <returns>The format result.</returns>
public delegate string FormatGroupNameCallback( string groupName, string apiVersion );