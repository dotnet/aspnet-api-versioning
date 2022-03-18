// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions
{
    using System.Reflection;

    internal delegate bool ODataActionQueryOptionConventionLookup(
        MethodInfo action,
        ODataQueryOptionSettings settings,
        out IODataQueryOptionsConvention? convention );
}