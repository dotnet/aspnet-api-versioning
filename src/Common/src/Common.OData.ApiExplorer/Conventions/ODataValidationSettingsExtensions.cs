// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.OData.Query.Validator;
#endif

internal static class ODataValidationSettingsExtensions
{
    internal static void CopyFrom( this ODataValidationSettings original, ODataValidationSettings source )
    {
        original.AllowedArithmeticOperators = source.AllowedArithmeticOperators;
        original.AllowedFunctions = source.AllowedFunctions;
        original.AllowedLogicalOperators = source.AllowedLogicalOperators;
        original.AllowedQueryOptions = source.AllowedQueryOptions;
        original.MaxAnyAllExpressionDepth = source.MaxAnyAllExpressionDepth;
        original.MaxExpansionDepth = source.MaxExpansionDepth;
        original.MaxNodeCount = source.MaxNodeCount;
        original.MaxOrderByNodeCount = source.MaxOrderByNodeCount;

        if ( source.MaxSkip.NoLimitOrNone() )
        {
            original.MaxSkip = source.MaxSkip;
        }

        if ( source.MaxTop.NoLimitOrSome() )
        {
            original.MaxTop = source.MaxTop;
        }

        var originalAllowedOrderByProperties = original.AllowedOrderByProperties;
        var sourceAllowedOrderByProperties = source.AllowedOrderByProperties;

        originalAllowedOrderByProperties.Clear();
#if NETFRAMEWORK
        for ( var i = 0; i < sourceAllowedOrderByProperties.Count; i++ )
        {
            originalAllowedOrderByProperties.Add( sourceAllowedOrderByProperties[i] );
        }
#else
        foreach ( var property in sourceAllowedOrderByProperties )
        {
            originalAllowedOrderByProperties.Add( property );
        }
#endif
    }
}