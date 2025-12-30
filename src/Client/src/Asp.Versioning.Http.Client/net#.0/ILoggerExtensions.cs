// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using static Microsoft.Extensions.Logging.LogLevel;

internal static partial class ILoggerExtensions
{
    internal static void ApiVersionDeprecated(
        this ILogger logger,
        Uri requestUrl,
        ApiVersion apiVersion,
        SunsetPolicy sunsetPolicy,
        DeprecationPolicy deprecationPolicy )
    {
        if ( !logger.IsEnabled( Warning ) )
        {
            return;
        }

        var sunsetDate = FormatDate( sunsetPolicy.Date );
        var deprecationDate = FormatDate( deprecationPolicy.Date );

        var additionalInfoSunset = FormatLinks( sunsetPolicy );
        var additionalInfoDeprecation = FormatLinks( deprecationPolicy );

        var additionalInfo = additionalInfoDeprecation.Concat( additionalInfoSunset ).ToArray();

#pragma warning disable IDE0079
#pragma warning disable CA1873

        ApiVersionDeprecated(
            logger,
            apiVersion.ToString(),
            requestUrl.OriginalString,
            sunsetDate,
            deprecationDate,
            additionalInfo );

#pragma warning restore CA1873
#pragma warning restore IDE0079
    }

    [LoggerMessage( EventId = 1, Level = Warning, Message = "API version {apiVersion} for {requestUrl} has been deprecated since {deprecationDate} and will sunset on {sunsetDate}. Additional information: {links}" )]
    static partial void ApiVersionDeprecated(
        ILogger logger,
        string apiVersion,
        string requestUrl,
        string sunsetDate,
        string deprecationDate,
        string[] links );

    internal static void NewApiVersionAvailable(
        this ILogger logger,
        Uri requestUrl,
        ApiVersion currentApiVersion,
        ApiVersion newApiVersion,
        SunsetPolicy sunsetPolicy )
    {
        if ( !logger.IsEnabled( Information ) )
        {
            return;
        }

        var sunsetDate = FormatDate( sunsetPolicy.Date );
        var additionalInfo = FormatLinks( sunsetPolicy );

#pragma warning disable IDE0079
#pragma warning disable CA1873

        NewApiVersionAvailable(
            logger,
            newApiVersion.ToString(),
            requestUrl.OriginalString,
            currentApiVersion.ToString(),
            sunsetDate,
            additionalInfo );

#pragma warning restore CA1873
#pragma warning restore IDE0079
    }

    [LoggerMessage( EventId = 2, Level = Information, Message = "API version {newApiVersion} is now available for {requestUrl} ({currentApiVersion}) until {sunsetDate}. Additional information: {links}" )]
    private static partial void NewApiVersionAvailable(
        ILogger logger,
        string newApiVersion,
        string requestUrl,
        string currentApiVersion,
        string sunsetDate,
        string[] links );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string FormatDate( DateTimeOffset? date ) =>
        date.HasValue ? date.Value.ToString( CultureInfo.CurrentCulture ) : "<unspecified>";

    private static string[] FormatLinks( SunsetPolicy sunsetPolicy )
    {
        if ( !sunsetPolicy.HasLinks )
        {
            return [];
        }

        return FormatLinks( sunsetPolicy.Links );
    }

    private static string[] FormatLinks( DeprecationPolicy deprecationPolicy )
    {
        if ( !deprecationPolicy.HasLinks )
        {
            return [];
        }

        return FormatLinks( deprecationPolicy.Links );
    }

    private static string[] FormatLinks( IList<LinkHeaderValue> links )
    {
        // <Title> (<Language>[,<Language>]): <Url>
        var text = new StringBuilder();
        var additionalInfo = new string[links.Count];

        for ( var i = 0; i < links.Count; i++ )
        {
            var link = links[i];

            text.Clear();

            if ( !StringSegment.IsNullOrEmpty( link.Title ) )
            {
#if NETSTANDARD2_0
                text.Append( link.Title.ToString() );
#else
                text.Append( link.Title.AsSpan() );
#endif
            }

            if ( link.Languages.Count > 0 )
            {
                if ( text.Length > 0 )
                {
                    text.Append( ' ' );
                }

                var languages = link.Languages;

                text.Append( '(' );
#if NETSTANDARD2_0
                text.Append( languages[0].ToString() );
#else
                text.Append( languages[0].AsSpan() );
#endif
                for ( var j = 1; j < languages.Count; j++ )
                {
                    text.Append( ',' )
#if NETSTANDARD2_0
                        .Append( languages[j].ToString() );
#else
                        .Append( languages[j].AsSpan() );
#endif
                }

                text.Append( ')' );
            }

            if ( text.Length > 0 )
            {
                text.Append( ": " );
            }

            text.Append( link.LinkTarget.OriginalString );

            additionalInfo[i] = text.ToString();
        }

        return additionalInfo;
    }
}