// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi;

using System.Globalization;
using System.Text;

/// <summary>
/// Represents the options used to format the OpenAPI document title.
/// </summary>
public class OpenApiDocumentDescriptionOptions
{
    private static CompositeFormat? deprecatedNoticeFormat;
    private static CompositeFormat? sunsetNoticeFormat;

    /// <summary>
    /// Gets or sets a value indicating whether API versioning policy links are hidden.
    /// </summary>
    /// <value>True if API versioning policy links are hidden; otherwise, false. The default value is
    /// <c>false</c>.</value>
    /// <remarks>Only API versioning policy links with the media type <c>text/html</c> will be shown.</remarks>
    public bool HidePolicyLinks { get; set; }

    /// <summary>
    /// Gets or sets the function used to generate the API versioning deprecation notice based on the provided policy.
    /// </summary>
    /// <value>The <see cref="Func{T, TResult}">function</see> used to generate the deprecation notice.</value>
    /// <remarks>If the function generates a <c>null</c> or empty message, then no notice is displayed.</remarks>
    public Func<DeprecationPolicy, string?> DeprecationNotice { get; set; } = DefaultDeprecationNotice;

    /// <summary>
    /// Gets or sets the function used to generate the API versioning sunset notice based on the provided policy.
    /// </summary>
    /// <value>The <see cref="Func{T, TResult}">function</see> used to generate the sunset notice.</value>
    /// <remarks>If the function generates a <c>null</c> or empty message, then no notice is displayed.</remarks>
    public Func<SunsetPolicy, string?> SunsetNotice { get; set; } = DefaultSunsetNotice;

    private static string? DefaultDeprecationNotice( DeprecationPolicy policy )
    {
        if ( policy.Date is not { } when )
        {
            return SR.DeprecatedNotice;
        }

        var participle = when < DateTimeOffset.Now ? SR.Was : SR.WillBe;
        deprecatedNoticeFormat ??= CompositeFormat.Parse( SR.DeprecatedNoticeFormat );
        return string.Format( CultureInfo.CurrentCulture, deprecatedNoticeFormat, participle, when );
    }

    private static string? DefaultSunsetNotice( SunsetPolicy policy )
    {
        if ( policy.Date is not { } when )
        {
            return default;
        }

        var participle = when < DateTimeOffset.Now ? SR.Was : SR.WillBe;
        sunsetNoticeFormat ??= CompositeFormat.Parse( SR.SunsetNoticeFormat );
        return string.Format( CultureInfo.CurrentCulture, sunsetNoticeFormat, participle, when );
    }
}