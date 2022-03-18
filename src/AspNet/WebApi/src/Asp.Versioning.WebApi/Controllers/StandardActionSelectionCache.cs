// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Asp.Versioning.Routing;
using System.Web.Http.Controllers;

internal sealed class StandardActionSelectionCache
{
    internal ILookup<string, HttpActionDescriptor>? StandardActionNameMapping { get; set; }

    internal CandidateAction[]? StandardCandidateActions { get; set; }

    internal CandidateAction[][]? CacheListMethods { get; set; }
}