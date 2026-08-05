// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable IDE0056
#pragma warning disable IDE0057
#pragma warning disable SA1121

namespace Asp.Versioning;

using static System.Globalization.CultureInfo;
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

/// <summary>
/// Represents an API version range.
/// </summary>
/// <remarks>This class is used to match API version ranges. It is not intended to define an API version range. API
/// versions must be explicitly declared.</remarks>
#if ANALYZER
internal
#else
public
#endif
sealed partial class ApiVersionRange
{
    private static ApiVersionRange? any;
    private static ApiVersionRange? empty;
    private readonly IRule rule;

    private ApiVersionRange( IRule rule ) => this.rule = rule;

    /// <summary>
    /// Gets a range that contains any API versions.
    /// </summary>
    /// <value>An empty <see cref="ApiVersionRange">API version range</see>, which always matches.</value>
    public static ApiVersionRange Any => any ??= new( new Constant( true ) );

    /// <summary>
    /// Gets a range that never contains any API versions.
    /// </summary>
    /// <value>An empty <see cref="ApiVersionRange">API version range</see>, which never matches.</value>
    public static ApiVersionRange Empty => empty ??= new( new Constant( false ) );

    /// <summary>
    /// Parses an API version range from a set of range rules.
    /// </summary>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    /// <returns>A new <see cref="ApiVersionRange">API version range</see>.</returns>
    /// <remarks>
    /// <para>
    /// An API version range rule is expressed with the same syntax as a package version, but the version value is
    /// instead a valid API version. The interval notation for specifying API version ranges is summarized as follows:
    /// </para>
    /// <list type="table">
    ///  <listheader><term>Notation</term><term>Applied Rule</term><term>Description</term></listheader>
    ///  <item><term>1.0</term><term>x ≥ 1.0</term><description>Minimum version, inclusive</description></item>
    ///  <item><term>[1.0,)</term><term>x ≥ 1.0</term><description>Minimum version, inclusive</description></item>
    ///  <item><term>(1.0,)</term><term>x > 1.0</term><description>Minimum version, exclusive</description></item>
    ///  <item><term>[1.0]</term><term>x == 1.0</term><description>Exact version match</description></item>
    ///  <item><term>(,1.0]</term><term>x ≤ 1.0</term><description>Maximum version, inclusive</description></item>
    ///  <item><term>(,1.0)</term><term>x &lt; 1.0</term><description>Maximum version, exclusive</description></item>
    ///  <item><term>[1.0,2.0]</term><term>1.0 ≤ x ≤ 2.0</term><description>Exact range, inclusive</description></item>
    ///  <item><term>(1.0,2.0)</term><term>1.0 &lt; x &lt; 2.0</term><description>Exact range, exclusive</description></item>
    ///  <item><term>[1.0,2.0)</term><term>1.0 ≤ x &lt; 2.0</term><description>Mixed inclusive minimum and exclusive maximum version</description></item>
    ///  <item><term>(1.0)</term><term>invalid</term><description>invalid</description></item>
    /// </list>
    /// <para>
    /// When <paramref name="otherRules"/> are specified, each range is combined as a logical or.
    /// </para>
    /// </remarks>
    public static ApiVersionRange Parse( string rule, params string[] otherRules )
        => Parse( ApiVersionParser.Default, rule, otherRules );

    /// <summary>
    /// Parses an API version range from a set of range rules.
    /// </summary>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used in rule construction.</param>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    /// <returns>A new <see cref="ApiVersionRange">API version range</see>.</returns>
    /// <remarks>
    /// <para>
    /// An API version range rule is expressed with the same syntax as a package version, but the version value is
    /// instead a valid API version. The interval notation for specifying API version ranges is summarized as follows:
    /// </para>
    /// <list type="table">
    ///  <listheader><term>Notation</term><term>Applied Rule</term><term>Description</term></listheader>
    ///  <item><term>1.0</term><term>x ≥ 1.0</term><description>Minimum version, inclusive</description></item>
    ///  <item><term>[1.0,)</term><term>x ≥ 1.0</term><description>Minimum version, inclusive</description></item>
    ///  <item><term>(1.0,)</term><term>x > 1.0</term><description>Minimum version, exclusive</description></item>
    ///  <item><term>[1.0]</term><term>x == 1.0</term><description>Exact version match</description></item>
    ///  <item><term>(,1.0]</term><term>x ≤ 1.0</term><description>Maximum version, inclusive</description></item>
    ///  <item><term>(,1.0)</term><term>x &lt; 1.0</term><description>Maximum version, exclusive</description></item>
    ///  <item><term>[1.0,2.0]</term><term>1.0 ≤ x ≤ 2.0</term><description>Exact range, inclusive</description></item>
    ///  <item><term>(1.0,2.0)</term><term>1.0 &lt; x &lt; 2.0</term><description>Exact range, exclusive</description></item>
    ///  <item><term>[1.0,2.0)</term><term>1.0 ≤ x &lt; 2.0</term><description>Mixed inclusive minimum and exclusive maximum version</description></item>
    ///  <item><term>(1.0)</term><term>invalid</term><description>invalid</description></item>
    /// </list>
    /// <para>
    /// When <paramref name="otherRules"/> are specified, each range is combined as a logical or.
    /// </para>
    /// </remarks>
    public static ApiVersionRange Parse( IApiVersionParser parser, string rule, params string[] otherRules )
    {
        ArgumentNullException.ThrowIfNull( parser );
        ArgumentException.ThrowIfNullOrEmpty( rule );
#if NETSTANDARD1_0
        var ruleSet = ParseRule( parser, rule );
#else
        var ruleSet = ParseRule( parser, rule.AsSpan() );
#endif

        if ( otherRules is not null )
        {
            for ( var i = 0; i < otherRules.Length; i++ )
            {
                if ( otherRules[i] is not { } otherRule )
                {
                    continue;
                }

#if NETSTANDARD1_0
                ruleSet = Or( ruleSet, ParseRule( parser, otherRule ) );
#else
                ruleSet = Or( ruleSet, ParseRule( parser, otherRule.AsSpan() ) );
#endif
            }
        }

        return new( ruleSet );
    }

    /// <summary>
    /// Parses an API version range from a set of range rules.
    /// </summary>
    /// <param name="rules">The ranges to parse.</param>
    /// <returns>A new <see cref="ApiVersionRange">API version range</see>.</returns>
    /// <remarks>If <paramref name="rules"/> is empty, then <see cref="Empty"/> is returned.</remarks>
    public static ApiVersionRange Parse( IEnumerable<string> rules ) => Parse( ApiVersionParser.Default, rules );

    /// <summary>
    /// Parses an API version range from a set of range rules.
    /// </summary>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used in rule construction.</param>
    /// <param name="rules">The ranges to parse.</param>
    /// <returns>A new <see cref="ApiVersionRange">API version range</see>.</returns>
    /// <remarks>If <paramref name="rules"/> is empty, then <see cref="Empty"/> is returned.</remarks>
    public static ApiVersionRange Parse( IApiVersionParser parser, IEnumerable<string> rules )
    {
        ArgumentNullException.ThrowIfNull( parser );
        ArgumentNullException.ThrowIfNull( rules );

        var list = rules.ToList();

        if ( list.Count == 0 )
        {
            return Empty;
        }

        var rule = list[0];

        list.RemoveAt( 0 );

        return Parse( parser, rule, [.. list] );
    }

    /// <summary>
    /// Determines whether the range contains the specified API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
    /// <returns><c>true</c> if the range contain the <paramref name="apiVersion"/>; otherwise <c>false</c>.</returns>
    public bool Contains( ApiVersion apiVersion ) => rule.Evaluate( apiVersion );

    private static FormatException NewInvalidRule( string rule )
        => new( string.Format( InvariantCulture, Format.InvalidApiVersionRange, rule ) );

    private static IRule ParseRule( IApiVersionParser parser, Text rule )
    {
#if NETSTANDARD1_0
        var segments = rule.Split( [','], 2 );
        var simple = segments.Length == 1;
#else
        var mid = rule.IndexOf( ',' );
        var simple = mid == -1;
#endif
        if ( simple )
        {
            return ParseMinOrExact( parser, rule );
        }

#if NETSTANDARD1_0
        var left = segments[0].TrimEnd();
        var right = segments[1].TrimStart();
#else
        var left = rule.Slice( 0, mid ).TrimEnd();
        var right = rule.Slice( mid + 1 ).TrimStart();
#endif

        if ( TryParseLower( parser, left, out var lower )
             && TryParseUpper( parser, right, out var upper ) )
        {
            if ( lower is null )
            {
                if ( upper is null )
                {
                    throw NewInvalidRule( rule.ToString() );
                }
                else
                {
                    return upper;
                }
            }
            else if ( upper is null )
            {
                return lower;
            }

            return And( lower, upper );
        }

        throw NewInvalidRule( rule.ToString() );
    }

    private static IRule ParseMinOrExact( IApiVersionParser parser, Text rule )
    {
        var exact = false;

        if ( rule.Length > 2 && rule[0] == '[' && rule[rule.Length - 1] == ']' )
        {
            exact = true;
#if NETSTANDARD1_0
            rule = rule.Substring( 1, rule.Length - 2 );
#else
            rule = rule.Slice( 1, rule.Length - 2 );
#endif
        }

        if ( parser.TryParse( rule, out var version ) )
        {
            return exact ? new Exact( version! ) : new MinInclusive( version! );
        }
        else
        {
            throw NewInvalidRule( rule.ToString() );
        }
    }

    private static Func<ApiVersion, IRule>? NewLowerRule( char ch ) => ch switch
    {
        '[' => static version => new MinInclusive( version ),
        '(' => static version => new MinExclusive( version ),
        _ => default,
    };

    private static Func<ApiVersion, IRule>? NewUpperRule( char ch ) => ch switch
    {
        ']' => static version => new MaxInclusive( version ),
        ')' => static version => new MaxExclusive( version ),
        _ => default,
    };

    private static bool TryParseLower( IApiVersionParser parser, Text expression, out IRule? rule )
    {
        if ( expression.Length == 0 || NewLowerRule( expression[0] ) is not { } newRule )
        {
            rule = default;
            return false;
        }

#if NETSTANDARD1_0
        expression = expression.Substring( 1 );
#else
        expression = expression.Slice( 1 );
#endif

        if ( expression.Length == 0 )
        {
            rule = default;
            return true;
        }
        else if ( parser.TryParse( expression, out var version ) )
        {
            rule = newRule( version! );
            return true;
        }

        rule = default;
        return false;
    }

    private static bool TryParseUpper( IApiVersionParser parser, Text expression, out IRule? rule )
    {
        var length = expression.Length - 1;

        if ( length < 0 || NewUpperRule( expression[length] ) is not { } newRule )
        {
            rule = default;
            return false;
        }

#if NETSTANDARD1_0
        expression = expression.Substring( 0, length );
#else
        expression = expression.Slice( 0, length );
#endif

        if ( expression.Length == 0 )
        {
            rule = default;
            return true;
        }
        else if ( parser.TryParse( expression, out var version ) )
        {
            rule = newRule( version! );
            return true;
        }

        rule = default;
        return false;
    }

    private static LogicalAnd And( IRule rule, IRule other ) => new( rule, other );

    private static LogicalOr Or( IRule rule, IRule other ) => new( rule, other );

    private interface IRule
    {
        bool Evaluate( ApiVersion apiVersion );
    }

    private sealed class Constant( bool match ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => match;
    }

    private sealed class Exact( ApiVersion minApiVersion ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => apiVersion == minApiVersion;
    }

    private sealed class MinInclusive( ApiVersion minApiVersion ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => apiVersion >= minApiVersion;
    }

    private sealed class MinExclusive( ApiVersion minApiVersion ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => apiVersion > minApiVersion;
    }

    private sealed class MaxInclusive( ApiVersion maxApiVersion ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => apiVersion <= maxApiVersion;
    }

    private sealed class MaxExclusive( ApiVersion maxApiVersion ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => apiVersion < maxApiVersion;
    }

    private sealed class LogicalAnd( IRule left, IRule right ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => left.Evaluate( apiVersion ) && right.Evaluate( apiVersion );
    }

    private sealed class LogicalOr( IRule left, IRule right ) : IRule
    {
        public bool Evaluate( ApiVersion apiVersion ) => left.Evaluate( apiVersion ) || right.Evaluate( apiVersion );
    }
}