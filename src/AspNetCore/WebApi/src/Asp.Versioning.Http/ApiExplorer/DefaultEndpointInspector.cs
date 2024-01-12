// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Represents the default <see cref="IEndpointInspector">endpoint inspector</see>.
/// </summary>
[CLSCompliant(false)]
public sealed class DefaultEndpointInspector : IEndpointInspector
{
    /// <inheritdoc />
    public bool IsControllerAction( Endpoint endpoint ) => false;
}