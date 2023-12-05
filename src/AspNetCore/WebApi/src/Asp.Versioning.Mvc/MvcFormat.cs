// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal static class MvcFormat
{
    internal static readonly CompositeFormat ActionMethodNotFound = CompositeFormat.Parse( MvcSR.ActionMethodNotFound );
    internal static readonly CompositeFormat AmbiguousActionMethod = CompositeFormat.Parse( MvcSR.AmbiguousActionMethod );
    internal static readonly CompositeFormat MultipleApiVersionsInferredFromNamespaces = CompositeFormat.Parse( MvcSR.MultipleApiVersionsInferredFromNamespaces );
    internal static readonly CompositeFormat InvalidActionMethodExpression = CompositeFormat.Parse( MvcSR.InvalidActionMethodExpression );
    internal static readonly CompositeFormat ConventionStyleMismatch = CompositeFormat.Parse( MvcSR.ConventionStyleMismatch );
}