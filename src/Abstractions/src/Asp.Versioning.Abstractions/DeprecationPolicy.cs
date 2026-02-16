// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents an API version deprecation policy.
/// </summary>
public class DeprecationPolicy
{
    private LinkList? links;

    /// <summary>
    /// Gets a read-only list of links that provide information about the deprecation policy.
    /// </summary>
    /// <value>A read-only list of HTTP links.</value>
    /// <remarks>If a link is provided, generally only one link is necessary; however, additional
    /// links might be provided for different languages or different formats such as a HTML page
    /// or a JSON file.</remarks>
    public IList<LinkHeaderValue> Links => links ??= new( "deprecation" );

    /// <summary>
    /// Gets a value indicating whether the deprecation policy has any associated links.
    /// </summary>
    /// <value>True if the deprecation policy has associated links; otherwise, false.</value>
    public bool HasLinks => links is not null && links.Count > 0;

    /// <summary>
    /// Gets the date and time when the API version will be deprecated.
    /// </summary>
    /// <value>The date and time when the API version will be deprecated, if any.</value>
    public DateTimeOffset? Date { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    public DeprecationPolicy() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    /// <param name="date">The date and time when the API version will be deprecated.</param>
    /// <param name="link">The optional link which provides information about the deprecation policy.</param>
    public DeprecationPolicy( DateTimeOffset date, LinkHeaderValue? link = default )
    {
        Date = date;

        if ( link is not null )
        {
            Links.Add( link );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    /// <param name="link">The link which provides information about the deprecation policy.</param>
    public DeprecationPolicy( LinkHeaderValue link ) => Links.Add( link );

    /// <summary>
    /// Returns a value indicating if this policy is effective for the specified date and time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset">date and time</see> to evaluate.</param>
    /// <returns>True if the policy is effective; otherwise, false.</returns>
    public bool IsEffective( DateTimeOffset dateTime ) => Date is { } date ? date <= dateTime : true;
}