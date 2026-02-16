// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents an API version sunset policy.
/// </summary>
public class SunsetPolicy
{
    private LinkList? links;

    /// <summary>
    /// Gets a read-only list of links that provide information about the sunset policy.
    /// </summary>
    /// <value>A read-only list of HTTP links.</value>
    /// <remarks>If a link is provided, generally only one link is necessary; however, additional
    /// links might be provided for different languages or different formats such as a HTML page
    /// or a JSON file.</remarks>
    public IList<LinkHeaderValue> Links => links ??= new( "sunset" );

    /// <summary>
    /// Gets a value indicating whether the sunset policy has any associated links.
    /// </summary>
    /// <value>True if the sunset policy has associated links; otherwise, false.</value>
    public bool HasLinks => links is not null && links.Count > 0;

    /// <summary>
    /// Gets the date and time when the API version will be sunset.
    /// </summary>
    /// <value>The date and time when the API version will be sunset, if any.</value>
    public DateTimeOffset? Date { get; }

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
            Links.Add( link );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicy"/> class.
    /// </summary>
    /// <param name="link">The link which provides information about the sunset policy.</param>
    public SunsetPolicy( LinkHeaderValue link ) => Links.Add( link );
}