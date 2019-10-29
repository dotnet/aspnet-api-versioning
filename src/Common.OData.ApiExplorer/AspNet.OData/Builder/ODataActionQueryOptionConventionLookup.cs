namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Reflection;

    delegate bool ODataActionQueryOptionConventionLookup( MethodInfo action, ODataQueryOptionSettings settings, out IODataQueryOptionsConvention? convention );
}