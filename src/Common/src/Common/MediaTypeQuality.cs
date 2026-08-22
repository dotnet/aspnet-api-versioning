// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using MediaType = System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
#else
using MediaType = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
#endif

internal static class MediaTypeQuality
{
    // The weight is normalized to a real number in the range 0 through 1, where 0.001 is the least preferred and 1 is
    // the most preferred; a value of 0 means 'not acceptable'. If no 'q' parameter is present, the default weight is 1
    // REF: https://www.rfc-editor.org/rfc/rfc9110#section-12.4.2
    private const double DefaultWeight = 1.0;

    internal static bool IsAcceptable( MediaType mediaType ) => WeightOf( mediaType ) > 0d;

    // weights parsed from the same textual form yield the same value and an omitted parameter always yields the same
    // constant, so an exact comparison is appropriate here
    internal static bool SameRank( MediaType left, MediaType right ) => WeightOf( left ) == WeightOf( right );

    // the content-type header has no quality parameter, so it always has the maximum weight. only media types of the
    // same rank can be collated with it; anything lower is outranked
    internal static ICollection<MediaType> MaxRanked( ICollection<MediaType> mediaTypes )
    {
        var maxRanked = default( List<MediaType> );

        foreach ( var mediaType in mediaTypes.Where( mt => WeightOf( mt ) == DefaultWeight ) )
        {
            ( maxRanked ??= new( capacity: mediaTypes.Count ) ).Add( mediaType );
        }

        return maxRanked ?? [];
    }

    // an insertion sort is used because it is stable, which retains the order specified by the client for media types
    // of equal weight. the number of media types in a header is expected to be small, which makes the cost negligible
    internal static void SortDescending( IList<MediaType> mediaTypes )
    {
        var count = mediaTypes.Count;

        for ( var i = 1; i < count; i++ )
        {
            var mediaType = mediaTypes[i];
            var weight = WeightOf( mediaType );
            var j = i - 1;

            for ( ; j >= 0 && WeightOf( mediaTypes[j] ) < weight; j-- )
            {
                mediaTypes[j + 1] = mediaTypes[j];
            }

            mediaTypes[j + 1] = mediaType;
        }
    }

    private static double WeightOf( MediaType mediaType ) => mediaType.Quality ?? DefaultWeight;
}