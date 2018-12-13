namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Text;
    using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
    using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static System.Globalization.CultureInfo;

    /// <summary>
    /// Represents the default <see cref="IODataQueryOptionDescriptionProvider">OData query option description provider.</see>.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public class DefaultODataQueryOptionDescriptionProvider : IODataQueryOptionDescriptionProvider
    {
        const char Space = ' ';

        /// <inheritdoc />
        public string Describe( AllowedQueryOptions queryOption, ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            if ( ( queryOption < Filter || queryOption > Supported ) || ( queryOption != Filter && ( (int) queryOption % 2 != 0 ) ) )
            {
                throw new ArgumentException( SR.MultipleQueryOptionsNotAllowed, nameof( queryOption ) );
            }

            switch ( queryOption )
            {
                case Filter:
                    return DescribeFilter( context );
                case Expand:
                    return DescribeExpand( context );
                case Select:
                    return DescribeSelect( context );
                case OrderBy:
                    return DescribeOrderBy( context );
                case Top:
                    return DescribeTop( context );
                case Skip:
                    return DescribeSkip( context );
                case Count:
                    return DescribeCount( context );
            }

#pragma warning disable CA1308 // Normalize strings to uppercase
            throw new ArgumentException( SR.UnsupportedQueryOption.FormatDefault( queryOption.ToString().ToLowerInvariant() ), nameof( queryOption ) );
#pragma warning restore CA1308
        }

        /// <summary>
        /// Describes the $filter query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeFilter( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.FilterQueryOptionDesc );

            if ( context.MaxNodeCount > 1 )
            {
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.MaxExpressionDesc, context.MaxNodeCount );
            }

            AppendAllowedOptions( description, context );

            if ( context.AllowedFilterProperties.Count > 0 )
            {
                var properties = ToCommaSeparatedValues( context.AllowedFilterProperties );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedPropertiesDesc, properties );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $expand query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeExpand( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.ExpandQueryOptionDesc );

            if ( context.MaxExpansionDepth > 0 )
            {
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.MaxDepthDesc, context.MaxExpansionDepth );
            }

            if ( context.AllowedExpandProperties.Count > 0 )
            {
                var properties = ToCommaSeparatedValues( context.AllowedExpandProperties );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedPropertiesDesc, properties );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $select query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeSelect( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.SelectQueryOptionDesc );

            if ( context.AllowedSelectProperties.Count > 0 )
            {
                var properties = ToCommaSeparatedValues( context.AllowedSelectProperties );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedPropertiesDesc, properties );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $orderby query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeOrderBy( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.OrderByQueryOptionDesc );

            if ( context.MaxOrderByNodeCount > 1 )
            {
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.MaxExpressionDesc, context.MaxOrderByNodeCount );
            }

            if ( context.AllowedOrderByProperties.Count > 0 )
            {
                var properties = ToCommaSeparatedValues( context.AllowedOrderByProperties );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedPropertiesDesc, properties );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $top query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeTop( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.TopQueryOptionDesc );

            if ( context.MaxTop != null && context.MaxTop.Value > 0 )
            {
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.MaxValueDesc, context.MaxTop.Value );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $skip query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeSkip( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var description = new StringBuilder();

            description.Append( SR.SkipQueryOptionDesc );

            if ( context.MaxSkip != null && context.MaxSkip.Value > 0 )
            {
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.MaxValueDesc, context.MaxSkip.Value );
            }

            return description.ToString();
        }

        /// <summary>
        /// Describes the $count query option.
        /// </summary>
        /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
        /// <returns>The query option description.</returns>
        protected virtual string DescribeCount( ODataQueryOptionDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            return SR.CountQueryOptionDesc;
        }

        static void AppendAllowedOptions( StringBuilder description, ODataQueryOptionDescriptionContext context )
        {
            Contract.Requires( description != null );
            Contract.Requires( context != null );

            if ( context.AllowedLogicalOperators != AllowedLogicalOperators.None &&
                 context.AllowedLogicalOperators != AllowedLogicalOperators.All )
            {
                var operators = ToCommaSeparatedValues( EnumerateLogicalOperators( context.AllowedLogicalOperators ) );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedLogicalOperatorsDesc, operators );
            }

            if ( context.AllowedArithmeticOperators != AllowedArithmeticOperators.None &&
                 context.AllowedArithmeticOperators != AllowedArithmeticOperators.All )
            {
                var operators = ToCommaSeparatedValues( EnumerateArithmeticOperators( context.AllowedArithmeticOperators ) );
                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedArithmeticOperatorsDesc, operators );
            }

            if ( context.AllowedFunctions != AllowedFunctions.None &&
                 context.AllowedFunctions != AllowedFunctions.All )
            {
#pragma warning disable CA1308 // Normalize strings to uppercase
                var functions = context.AllowedFunctions.ToString().ToLowerInvariant();
#pragma warning restore CA1308

                description.Append( Space );
                description.AppendFormat( CurrentCulture, SR.AllowedFunctionsDesc, functions );
            }
        }

        static IEnumerable<string> EnumerateLogicalOperators( AllowedLogicalOperators logicalOperators )
        {
            if ( logicalOperators.HasFlag( Equal ) )
            {
                yield return "eq";
            }

            if ( logicalOperators.HasFlag( NotEqual ) )
            {
                yield return "ne";
            }

            if ( logicalOperators.HasFlag( GreaterThan ) )
            {
                yield return "gt";
            }

            if ( logicalOperators.HasFlag( GreaterThanOrEqual ) )
            {
                yield return "ge";
            }

            if ( logicalOperators.HasFlag( LessThan ) )
            {
                yield return "lt";
            }

            if ( logicalOperators.HasFlag( LessThanOrEqual ) )
            {
                yield return "le";
            }

            if ( logicalOperators.HasFlag( Has ) )
            {
                yield return "has";
            }

            if ( logicalOperators.HasFlag( And ) )
            {
                yield return "and";
            }

            if ( logicalOperators.HasFlag( Or ) )
            {
                yield return "or";
            }

            if ( logicalOperators.HasFlag( Not ) )
            {
                yield return "not";
            }
        }

        static IEnumerable<string> EnumerateArithmeticOperators( AllowedArithmeticOperators arithmeticOperators )
        {
            if ( arithmeticOperators.HasFlag( Add ) )
            {
                yield return "add";
            }

            if ( arithmeticOperators.HasFlag( Subtract ) )
            {
                yield return "sub";
            }

            if ( arithmeticOperators.HasFlag( Multiply ) )
            {
                yield return "mul";
            }

            if ( arithmeticOperators.HasFlag( Divide ) )
            {
                yield return "div";
            }

            if ( arithmeticOperators.HasFlag( Modulo ) )
            {
                yield return "mod";
            }
        }

        static string ToCommaSeparatedValues( IEnumerable<string> values )
        {
            Contract.Requires( values != null );
            Contract.Ensures( Contract.Result<string>() != null );

            using ( var iterator = values.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return string.Empty;
                }

                var csv = new StringBuilder();

                csv.Append( iterator.Current );

                while ( iterator.MoveNext() )
                {
                    csv.Append( ", " );
                    csv.Append( iterator.Current );
                }

                return csv.ToString();
            }
        }
    }
}