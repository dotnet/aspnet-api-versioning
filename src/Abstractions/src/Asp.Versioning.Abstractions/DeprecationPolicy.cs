// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents an API version deprecation policy.
/// </summary>
public class DeprecationPolicy
{
    private readonly LinkList links;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    public DeprecationPolicy()
    {
        links = new DeprecationLinkList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    /// <param name="date">The date and time when the API version will be deprecated.</param>
    /// <param name="link">The optional link which provides information about the deprecation policy.</param>
    public DeprecationPolicy( DateTimeOffset date, LinkHeaderValue? link = default )
        : this()
    {
        Date = date;

        if ( link is not null )
        {
            links.Add( link );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicy"/> class.
    /// </summary>
    /// <param name="link">The link which provides information about the deprecation policy.</param>
    public DeprecationPolicy( LinkHeaderValue link )
        : this()
    {
        links.Add( link );
    }

    /// <summary>
    /// Gets the date and time when the API version will be deprecated.
    /// </summary>
    /// <value>The date and time when the API version will be deprecated, if any.</value>
    public DateTimeOffset? Date { get; }

    /// <summary>
    /// Gets a value indicating whether the deprecation policy has any associated links.
    /// </summary>
    /// <value>True if the deprecation policy has associated links; otherwise, false.</value>
    public bool HasLinks => links.Count > 0;

    /// <summary>
    /// Gets a read-only list of links that provide information about the deprecation policy.
    /// </summary>
    /// <value>A read-only list of HTTP links.</value>
    /// <remarks>If a link is provided, generally only one link is necessary; however, additional
    /// links might be provided for different languages or different formats such as a HTML page
    /// or a JSON file.</remarks>
    public IList<LinkHeaderValue> Links => links;

    internal sealed class DeprecationLinkList : LinkList
    {
        protected override void EnsureRelationType( LinkHeaderValue item )
        {
            if ( !item.RelationType.Equals( "deprecation", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new ArgumentException( SR.InvalidDeprecationRelationType, nameof( item ) );
            }
        }
    }
}