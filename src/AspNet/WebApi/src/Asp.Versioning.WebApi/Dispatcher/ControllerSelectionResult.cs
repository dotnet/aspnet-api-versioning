// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using System.Web.Http.Controllers;

internal sealed class ControllerSelectionResult
{
    internal HttpControllerDescriptor? Controller { get; set; }

    internal string? ControllerName { get; set; }

    internal bool Succeeded => Controller != null;

    internal bool HasCandidates { get; set; }

    internal ApiVersion? RequestedVersion { get; set; }
}