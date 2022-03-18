// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Web.Http.Controllers;

internal sealed class ActionSelectionResult
{
    internal ActionSelectionResult( HttpActionDescriptor action ) => Action = action;

    internal ActionSelectionResult( Exception exception ) => Exception = exception;

    internal bool Succeeded => Exception == null;

    internal HttpActionDescriptor? Action { get; }

    internal Exception? Exception { get; }
}