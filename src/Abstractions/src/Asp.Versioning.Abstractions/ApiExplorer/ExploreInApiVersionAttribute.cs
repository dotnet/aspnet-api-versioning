// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1813

namespace Asp.Versioning.ApiExplorer;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata to indicate a data member should be explored in a particular an API is version-neutral.
/// </summary>
[AttributeUsage( Property, AllowMultiple = false, Inherited = true )]
public class ExploreInApiVersionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExploreInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used in rule construction.</param>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    protected ExploreInApiVersionAttribute( IApiVersionParser parser, string rule, params string[] otherRules )
    {
        Range = ApiVersionRange.Parse( parser, otherRules );
        Rules = Combine( rule, otherRules );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExploreInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="rule">The range to parse.</param>
    public ExploreInApiVersionAttribute( string rule )
    {
        Range = ApiVersionRange.Parse( rule );
        Rules = [rule];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExploreInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    public ExploreInApiVersionAttribute( string rule, params string[] otherRules )
    {
        Range = ApiVersionRange.Parse( rule, otherRules );
        Rules = Combine( rule, otherRules );
    }

    /// <summary>
    /// Gets the specified API version range rules.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of API version range rules.</value>
    public IReadOnlyList<string> Rules { get; }

    /// <summary>
    /// Gets the range of API versions the member is explored in.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionRange">API version range</see>.</value>
    public ApiVersionRange Range { get; }

    private static string[] Combine( string rule, string[] otherRules )
    {
        if ( otherRules is null || otherRules.Length == 0 )
        {
            return [rule];
        }
        else
        {
#if NETSTANDARD1_0
            var allRules = new List<string>( otherRules.Length + 1 )
            {
                rule,
            };

            allRules.AddRange( otherRules );
            return [.. allRules];
#else
            var allRules = new string[otherRules.Length + 1];

            allRules[0] = rule;
            Array.Copy( otherRules, 0, allRules, 1, otherRules.Length );
            return allRules;
#endif
        }
    }
}