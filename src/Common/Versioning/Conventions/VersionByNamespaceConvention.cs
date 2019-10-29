#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using static System.Text.RegularExpressions.RegexOptions;

    /// <summary>
    /// Represents a convention which applies an API to a controller by its defined namespace.
    /// </summary>
    public partial class VersionByNamespaceConvention
    {
        static string? GetRawApiVersion( string @namespace )
        {
            // 'v' | 'V' : [<year> '-' <month> '-' <day>] : [<major[.minor]>] : [<status>]
            // ex: v2018_04_01_1_1_Beta
            const string Pattern = @"[^\.]?[vV](\d{4})?_?(\d{2})?_?(\d{2})?_?(\d+)?_?(\d*)_?([a-zA-Z][a-zA-Z0-9]*)?[\.$]?";

            var match = Regex.Match( @namespace, Pattern, Singleline );
            var rawApiVersions = new List<string>();
            var text = new StringBuilder();

            while ( match.Success )
            {
                ExtractDateParts( match, text );
                ExtractNumericParts( match, text );
                ExtractStatusPart( match, text );

                if ( text.Length > 0 )
                {
                    rawApiVersions.Add( text.ToString() );
                }

                text.Clear();
                match = match.NextMatch();
            }

            return rawApiVersions.Count switch
            {
                0 => default,
                1 => rawApiVersions[0],
                _ => throw new InvalidOperationException( SR.MultipleApiVersionsInferredFromNamespaces.FormatInvariant( @namespace ) ),
            };
        }

        static void ExtractDateParts( Match match, StringBuilder text )
        {
            var year = match.Groups[1];
            var month = match.Groups[2];
            var day = match.Groups[3];

            if ( !year.Success || !month.Success || !day.Success )
            {
                return;
            }

            text.Append( year.Value );
            text.Append( '-' );
            text.Append( month.Value );
            text.Append( '-' );
            text.Append( day.Value );
        }

        static void ExtractNumericParts( Match match, StringBuilder text )
        {
            var major = match.Groups[4];

            if ( !major.Success )
            {
                return;
            }

            if ( text.Length > 0 )
            {
                text.Append( '.' );
            }

            text.Append( major.Value );

            var minor = match.Groups[5];

            if ( !minor.Success )
            {
                return;
            }

            text.Append( '.' );

            if ( minor.Length > 0 )
            {
                text.Append( minor.Value );
            }
            else
            {
                text.Append( '0' );
            }
        }

        static void ExtractStatusPart( Match match, StringBuilder text )
        {
            var status = match.Groups[6];

            if ( status.Success && text.Length > 0 )
            {
                text.Append( '-' );
                text.Append( status.Value );
            }
        }
    }
}