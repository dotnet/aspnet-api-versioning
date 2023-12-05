// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal static class Format
{
    internal static readonly CompositeFormat UnsupportedQueryOption = CompositeFormat.Parse( ODataExpSR.UnsupportedQueryOption );
    internal static readonly CompositeFormat MaxExpressionDesc = CompositeFormat.Parse( ODataExpSR.MaxExpressionDesc );
    internal static readonly CompositeFormat AllowedPropertiesDesc = CompositeFormat.Parse( ODataExpSR.AllowedPropertiesDesc );
    internal static readonly CompositeFormat MaxDepthDesc = CompositeFormat.Parse( ODataExpSR.MaxDepthDesc );
    internal static readonly CompositeFormat MaxValueDesc = CompositeFormat.Parse( ODataExpSR.MaxValueDesc );
    internal static readonly CompositeFormat AllowedLogicalOperatorsDesc = CompositeFormat.Parse( ODataExpSR.AllowedLogicalOperatorsDesc );
    internal static readonly CompositeFormat AllowedArithmeticOperatorsDesc = CompositeFormat.Parse( ODataExpSR.AllowedArithmeticOperatorsDesc );
    internal static readonly CompositeFormat AmbiguousActionMethod = CompositeFormat.Parse( ODataExpSR.AmbiguousActionMethod );
    internal static readonly CompositeFormat InvalidActionMethodExpression = CompositeFormat.Parse( ODataExpSR.InvalidActionMethodExpression );
    internal static readonly CompositeFormat ActionMethodNotFound = CompositeFormat.Parse( ODataExpSR.ActionMethodNotFound );
    internal static readonly CompositeFormat AllowedFunctionsDesc = CompositeFormat.Parse( ODataExpSR.AllowedFunctionsDesc );
    internal static readonly CompositeFormat RequiredInterfaceNotImplemented = CompositeFormat.Parse( ODataExpSR.RequiredInterfaceNotImplemented );
    internal static readonly CompositeFormat ConventionStyleMismatch = CompositeFormat.Parse( ODataExpSR.ConventionStyleMismatch );
}