namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using System;

    static class ODataValidationSettingsExtensions
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

            if ( source.MaxSkip != null )
            {
                original.MaxSkip = source.MaxSkip;
            }

            if ( source.MaxTop != null )
            {
                original.MaxTop = source.MaxTop;
            }

            original.AllowedOrderByProperties.Clear();

            foreach ( var property in source.AllowedOrderByProperties )
            {
                original.AllowedOrderByProperties.Add( property );
            }
        }
    }
}