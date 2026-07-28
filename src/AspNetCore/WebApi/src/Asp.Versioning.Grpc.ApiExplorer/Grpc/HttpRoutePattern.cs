// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Asp.Versioning.ApiExplorer;
using System.Text;

internal sealed class HttpRoutePattern(
    List<string> segments, string? method,
    List<HttpRouteVariable> variables,
    GrpcApiExplorerOptions options )
{
    public List<string> Segments { get; } = segments;

    public string? Method { get; } = method;

    public List<HttpRouteVariable> Variables { get; } = variables;

    public static HttpRoutePattern Parse( string pattern, GrpcApiExplorerOptions options )
    {
        var parser = new HttpRoutePatternParser( pattern );

        parser.Parse();

        return new HttpRoutePattern( parser.Segments, parser.Method, parser.Variables, options );
    }

    public string BuildPath( Dictionary<string, RouteParameter> parameters )
    {
        var path = new StringBuilder();

        for ( var i = 0; i < Segments.Count; i++ )
        {
            if ( path.Length > 0 )
            {
                path.Append( '/' );
            }

            if ( parameters.SingleOrDefault( kvp => kvp.Value.RouteVariable.StartSegment == i ).Value is { } parameter )
            {
                if ( parameter.DescriptorsPath.Count > 0
                    && StringComparer.Ordinal.Equals( parameter.DescriptorsPath[0].Name, options.RouteParameter.Name ) )
                {
                    path.Append( options.RouteParameter.PrefixLiteral );
                }

                path.Append( '{' ).Append( parameter.JsonPath ).Append( '}' );

                // skip segments if variable is multiple segment
                i = parameter.RouteVariable.EndSegment - 1;
            }
            else
            {
                path.Append( Segments[i] );
            }
        }

        if ( Method != null )
        {
            path.Append( ':' );
            path.Append( Method );
        }

        return path.ToString();
    }
}