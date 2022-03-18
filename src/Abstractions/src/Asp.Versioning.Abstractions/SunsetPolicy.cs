// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Collections.ObjectModel;

/// <summary>
/// Represents an API version sunset policy.
/// </summary>
public class SunsetPolicy
{
    private SunsetLinkList? links;

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicy"/> class.
    /// </summary>
    public SunsetPolicy() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicy"/> class.
    /// </summary>
    /// <param name="date">The date and time when the API version will be sunset.</param>
    /// <param name="link">The optional link which provides information about the sunset policy.</param>
    public SunsetPolicy( DateTimeOffset date, LinkHeaderValue? link = default )
    {
        Date = date;

        if ( link is not null )
        {
            links = new() { link };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicy"/> class.
    /// </summary>
    /// <param name="link">The link which provides information about the sunset policy.</param>
    public SunsetPolicy( LinkHeaderValue link ) => links = new() { link };

    /// <summary>
    /// Gets the date and time when the API version will be sunset.
    /// </summary>
    /// <value>The date and time when the API version will be sunset, if any.</value>
    public DateTimeOffset? Date { get; }

    /// <summary>
    /// Gets a value indicating whether the sunset policy has any associated links.
    /// </summary>
    /// <value>True if the sunset policy has associated links; otherwise, false.</value>
    public bool HasLinks => links is not null && links.Count > 0;

    /// <summary>
    /// Gets a read-only list of links that provide information about the sunset policy.
    /// </summary>
    /// <value>A read-only list of HTTP links.</value>
    /// <remarks>If a link is provided, generally only one link is necessary; however, additional
    /// links might be provided for different languages or different formats such as a HTML page
    /// or a JSON file.</remarks>
    public IList<LinkHeaderValue> Links => links ??= new();

    private sealed class SunsetLinkList : Collection<LinkHeaderValue>
    {
        protected override void InsertItem( int index, LinkHeaderValue item )
        {
            base.InsertItem( index, item );
            EnsureRelationType( item );
        }

        protected override void SetItem( int index, LinkHeaderValue item )
        {
            base.SetItem( index, item );
            EnsureRelationType( item );
        }

        private static void EnsureRelationType( LinkHeaderValue item )
        {
            if ( !item.RelationType.Equals( "sunset", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new ArgumentException( SR.InvalidSunsetRelationType, nameof( item ) );
            }
        }
    }
}