// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata to indicate an API is version-neutral.
/// </summary>
[AttributeUsage( Class | Method, AllowMultiple = false, Inherited = true )]
public sealed class ApiVersionNeutralAttribute : Attribute, IApiVersionNeutral
{
}