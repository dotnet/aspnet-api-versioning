// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1813

namespace Asp.Versioning;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata to indicate whether a data member should be visible in a particular API version.
/// </summary>
[AttributeUsage( Property, AllowMultiple = false, Inherited = true )]
public class VisibleInApiVersionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VisibleInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used in rule construction.</param>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    /// <remarks>See <seealso cref="ApiVersionRange"/> for more information on rule notation.</remarks>
    protected VisibleInApiVersionAttribute( IApiVersionParser parser, string rule, params string[] otherRules )
        => Range = ApiVersionRange.Parse( parser, rule, otherRules );

    /// <summary>
    /// Initializes a new instance of the <see cref="VisibleInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="rule">The range to parse.</param>
    /// <remarks>See <seealso cref="ApiVersionRange"/> for more information on rule notation.</remarks>
    public VisibleInApiVersionAttribute( string rule ) => Range = ApiVersionRange.Parse( rule );

    /// <summary>
    /// Initializes a new instance of the <see cref="VisibleInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="rule">The range to parse.</param>
    /// <param name="otherRules">Additional ranges to parse, if any.</param>
    /// <remarks>See <seealso cref="ApiVersionRange"/> for more information on rule notation.</remarks>
    public VisibleInApiVersionAttribute( string rule, params string[] otherRules )
        => Range = ApiVersionRange.Parse( rule, otherRules );

    /// <summary>
    /// Gets the range of API versions the member is explored in.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionRange">API version range</see>.</value>
    public ApiVersionRange Range { get; }
}