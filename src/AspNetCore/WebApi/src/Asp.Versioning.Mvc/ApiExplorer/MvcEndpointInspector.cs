// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents the <see cref="IEndpointInspector">inspector</see> that understands
/// <see cref="Endpoint">endpoints</see> defined by MVC controllers.
/// </summary>
[CLSCompliant(false)]
public sealed class MvcEndpointInspector : IEndpointInspector
{
    /// <inheritdoc />
    public bool IsControllerAction( Endpoint endpoint )
    {
        ArgumentNullException.ThrowIfNull( endpoint );
        return endpoint.Metadata.Any( static attribute => attribute is ControllerAttribute );
    }
}