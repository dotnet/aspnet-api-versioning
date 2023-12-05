// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.OData.Query;
#endif
using System.Text;
#if NETFRAMEWORK
using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
#else
using static Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
#endif
using static System.Globalization.CultureInfo;
#if NETFRAMEWORK
using Fmt = Asp.Versioning.ODataExpSR;
#else
using Fmt = Asp.Versioning.Format;
#endif

/// <summary>
/// Represents the default <see cref="IODataQueryOptionDescriptionProvider">OData query option description provider.</see>.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class DefaultODataQueryOptionDescriptionProvider : IODataQueryOptionDescriptionProvider
{
    private const char Space = ' ';
    private StringBuilder? sharedBuilder;

    /// <inheritdoc />
    public virtual string Describe( AllowedQueryOptions queryOption, ODataQueryOptionDescriptionContext context )
    {
        if ( ( queryOption < Filter || queryOption > Supported ) ||
             ( queryOption != Filter && ( (int) queryOption % 2 != 0 ) ) )
        {
            throw new System.ArgumentException( ODataExpSR.MultipleQueryOptionsNotAllowed, nameof( queryOption ) );
        }

        return queryOption switch
        {
            Filter => DescribeFilter( context ),
            Expand => DescribeExpand( context ),
            Select => DescribeSelect( context ),
            OrderBy => DescribeOrderBy( context ),
            Top => DescribeTop( context ),
            Skip => DescribeSkip( context ),
            Count => DescribeCount( context ),
            _ => throw new System.ArgumentException(
                    string.Format(
                        CurrentCulture,
                        Fmt.UnsupportedQueryOption,
#pragma warning disable IDE0079
#pragma warning disable CA1308 // Normalize strings to uppercase (proper casing is lowercase)
                        queryOption.ToString().ToLowerInvariant() ),
#pragma warning restore CA1308 // Normalize strings to uppercase
#pragma warning restore IDE0079
                    nameof( queryOption ) ),
        };
    }

    /// <summary>
    /// Describes the $filter query option.
    /// </summary>
    /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
    /// <returns>The query option description.</returns>
    protected virtual string DescribeFilter( ODataQueryOptionDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var description = new StringBuilder();

        description.Append( ODataExpSR.FilterQueryOptionDesc );

        if ( context.MaxNodeCount > 1 )
        {
            description.Append( Space )
                       .AppendFormat( CurrentCulture, Fmt.MaxExpressionDesc, context.MaxNodeCount );
        }

        AppendAllowedOptions( description, context );

        if ( context.AllowedFilterProperties.Count > 0 )
        {
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedPropertiesDesc,
                            string.Join( ", ", context.AllowedFilterProperties ) );
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
        ArgumentNullException.ThrowIfNull( context );

        bool hasMaxExpansionDepth;

        if ( !( hasMaxExpansionDepth = context.MaxExpansionDepth > 0 ) &&
             context.AllowedExpandProperties.Count <= 0 )
        {
            return ODataExpSR.ExpandQueryOptionDesc;
        }

        var description = GetOrCreateBuilder().Append( ODataExpSR.ExpandQueryOptionDesc );

        if ( hasMaxExpansionDepth )
        {
            description.Append( Space )
                       .AppendFormat( CurrentCulture, Fmt.MaxDepthDesc, context.MaxExpansionDepth );
        }

        if ( context.AllowedExpandProperties.Count > 0 )
        {
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedPropertiesDesc,
                            string.Join( ", ", context.AllowedExpandProperties ) );
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
        ArgumentNullException.ThrowIfNull( context );

        if ( context.AllowedSelectProperties.Count <= 0 )
        {
            return ODataExpSR.SelectQueryOptionDesc;
        }

        return GetOrCreateBuilder()
                .Append( ODataExpSR.SelectQueryOptionDesc )
                .Append( Space )
                .AppendFormat(
                    CurrentCulture,
                    Fmt.AllowedPropertiesDesc,
                    string.Join( ", ", context.AllowedSelectProperties ) )
                .ToString();
    }

    /// <summary>
    /// Describes the $orderby query option.
    /// </summary>
    /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
    /// <returns>The query option description.</returns>
    protected virtual string DescribeOrderBy( ODataQueryOptionDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        bool hasMaxOrderByNodeCount;

        if ( !( hasMaxOrderByNodeCount = context.MaxOrderByNodeCount > 1 ) &&
             context.AllowedOrderByProperties.Count <= 0 )
        {
            return ODataExpSR.OrderByQueryOptionDesc;
        }

        var description = GetOrCreateBuilder().Append( ODataExpSR.OrderByQueryOptionDesc );

        if ( hasMaxOrderByNodeCount )
        {
            description.Append( Space )
                       .AppendFormat( CurrentCulture, Fmt.MaxExpressionDesc, context.MaxOrderByNodeCount );
        }

        if ( context.AllowedOrderByProperties.Count > 0 )
        {
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedPropertiesDesc,
                            string.Join( ", ", context.AllowedOrderByProperties ) );
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
        ArgumentNullException.ThrowIfNull( context );

        if ( context.MaxTop.NoLimitOrNone() )
        {
            return ODataExpSR.TopQueryOptionDesc;
        }

        return GetOrCreateBuilder()
                .Append( ODataExpSR.TopQueryOptionDesc )
                .Append( Space )
                .AppendFormat( CurrentCulture, Fmt.MaxValueDesc, context.MaxTop )
                .ToString();
    }

    /// <summary>
    /// Describes the $skip query option.
    /// </summary>
    /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
    /// <returns>The query option description.</returns>
    protected virtual string DescribeSkip( ODataQueryOptionDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        if ( context.MaxSkip.NoLimitOrNone() )
        {
            return ODataExpSR.SkipQueryOptionDesc;
        }

        return GetOrCreateBuilder()
                .Append( ODataExpSR.SkipQueryOptionDesc )
                .Append( Space )
                .AppendFormat( CurrentCulture, Fmt.MaxValueDesc, context.MaxSkip )
                .ToString();
    }

    /// <summary>
    /// Describes the $count query option.
    /// </summary>
    /// <param name="context">The current <see cref="ODataQueryOptionDescriptionContext">description context</see>.</param>
    /// <returns>The query option description.</returns>
    protected virtual string DescribeCount( ODataQueryOptionDescriptionContext context ) => ODataExpSR.CountQueryOptionDesc;

    private static void AppendAllowedOptions( StringBuilder description, ODataQueryOptionDescriptionContext context )
    {
        if ( context.AllowedLogicalOperators != AllowedLogicalOperators.None &&
             context.AllowedLogicalOperators != AllowedLogicalOperators.All )
        {
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedLogicalOperatorsDesc,
                            string.Join(
                                ", ",
                                EnumerateLogicalOperators( context.AllowedLogicalOperators ) ) );
        }

        if ( context.AllowedArithmeticOperators != AllowedArithmeticOperators.None &&
             context.AllowedArithmeticOperators != AllowedArithmeticOperators.All )
        {
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedArithmeticOperatorsDesc,
                            string.Join(
                                ", ",
                                EnumerateArithmeticOperators( context.AllowedArithmeticOperators ) ) );
        }

        if ( context.AllowedFunctions != AllowedFunctions.None &&
             context.AllowedFunctions != AllowedFunctions.AllFunctions )
        {
#pragma warning disable IDE0079
#pragma warning disable CA1308 // Normalize strings to uppercase (proper casing is lowercase)
            description.Append( Space )
                       .AppendFormat(
                            CurrentCulture,
                            Fmt.AllowedFunctionsDesc,
                            context.AllowedFunctions.ToString().ToLowerInvariant() );
#pragma warning restore CA1308 // Normalize strings to uppercase
#pragma warning restore IDE0079
        }
    }

    private static IEnumerable<string> EnumerateLogicalOperators( AllowedLogicalOperators logicalOperators )
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

    private static IEnumerable<string> EnumerateArithmeticOperators( AllowedArithmeticOperators arithmeticOperators )
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

    private StringBuilder GetOrCreateBuilder()
    {
        if ( sharedBuilder == null )
        {
            sharedBuilder = new();
        }
        else
        {
            sharedBuilder.Clear();
        }

        return sharedBuilder;
    }
}