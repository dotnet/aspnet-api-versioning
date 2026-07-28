// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using System.Text;
using static System.Globalization.CultureInfo;

// HTTP Template Grammar:
//
// Template = "/" | "/" Segments [ Method ] ;
// Segments = Segment { "/" Segment } ;
// Segment  = "*" | "**" | LITERAL | Variable ;
// Variable = "{" FieldPath [ "=" Segments ] "}" ;
// FieldPath = IDENT { "." IDENT } ;
// Method     = ":" LITERAL ;
internal sealed class HttpRoutePatternParser( string input )
{
    internal static readonly CompositeFormat RoutePatternParseError = CompositeFormat.Parse( SR.RoutePatternParseError );
    internal static readonly CompositeFormat MissingTemplateChar = CompositeFormat.Parse( SR.MissingTemplateChar );
    private readonly string input = input;
    private readonly List<string> segments = [];
    private readonly List<HttpRouteVariable> variables = [];
    private string? method;
    private int tokenStart;
    private int tokenEnd;
    private bool inVariable;
    private bool hasCatchAllSegment;

    public List<string> Segments => segments;

    public string? Method => method;

    public List<HttpRouteVariable> Variables => variables;

    private char? CurrentChar => tokenStart < tokenEnd && tokenEnd <= input.Length ? input[tokenEnd - 1] : null;

    private HttpRouteVariable CurrentVariable
    {
        get
        {
            if ( !inVariable || variables.LastOrDefault() is not HttpRouteVariable variable )
            {
                throw new InvalidOperationException( SR.UnexpectedRouteVariableError );
            }

            return variable;
        }
    }

    public void Parse()
    {
        try
        {
            ParseTemplate();
        }
        catch ( InvalidOperationException ex )
        {
            throw new InvalidOperationException( string.Format( CurrentCulture, RoutePatternParseError, input ), ex );
        }

        if ( tokenStart < input.Length )
        {
            throw new InvalidOperationException( SR.UnparsedRoutePattern );
        }
    }

    // Template = "/" Segments [ Method ] ;
    private void ParseTemplate()
    {
        if ( !Consume( '/' ) )
        {
            throw new InvalidOperationException( SR.PathMustStartWithSlash );
        }

        ParseSegments();

        if ( EnsureCurrent() )
        {
            if ( CurrentChar != ':' )
            {
                throw new InvalidOperationException( SR.PathMustEndWithSlash );
            }

            ParseMethod();
        }
    }

    // Segments = Segment { "/" Segment } ;
    private void ParseSegments()
    {
        while ( true )
        {
            // Support '/' template.
            if ( !ParseSegment() && segments.Count > 0 )
            {
                throw new InvalidOperationException( SR.TemplateShouldNotEndWithSlash );
            }

            if ( !Consume( '/' ) )
            {
                break;
            }
        }
    }

    // Segment  = "*" | "**" | LITERAL | Variable ;
    private bool ParseSegment()
    {
        if ( !EnsureCurrent() )
        {
            return false;
        }

        switch ( CurrentChar )
        {
            case '*':
                {
                    if ( hasCatchAllSegment )
                    {
                        throw new InvalidOperationException( SR.LiteralOnlyAfterCatchAll );
                    }

                    ConsumeAndAssert( '*' );

                    // Check for '**'
                    if ( Consume( '*' ) )
                    {
                        segments.Add( "**" );
                        hasCatchAllSegment = true;

                        if ( inVariable )
                        {
                            CurrentVariable.HasCatchAllPath = true;
                        }

                        return true;
                    }
                    else
                    {
                        segments.Add( "*" );
                        return true;
                    }
                }

            case '{':
                if ( hasCatchAllSegment )
                {
                    throw new InvalidOperationException( SR.LiteralOnlyAfterCatchAll );
                }

                ParseVariable();
                return true;
            default:
                ParseLiteralSegment();
                return true;
        }
    }

    // Variable = "{" FieldPath [ "=" Segments ] "}" ;
    private void ParseVariable()
    {
        ConsumeAndAssert( '{' );
        StartVariable();
        ParseFieldPath();

        if ( Consume( '=' ) )
        {
            ParseSegments();
        }
        else
        {
            segments.Add( "*" );
        }

        EndVariable();
        ConsumeAndAssert( '}' );
    }

    private void ParseLiteralSegment()
    {
        if ( !TryParseLiteral( out var literal ) )
        {
            throw new InvalidOperationException( SR.EmptyLiteral );
        }

        segments.Add( literal );
    }

    // FieldPath = IDENT { "." IDENT } ;
    private void ParseFieldPath()
    {
        do
        {
            if ( !ParseIdentifier() )
            {
                throw new InvalidOperationException( SR.EmptyFieldPath );
            }
        }
        while ( Consume( '.' ) );
    }

    // Method     = ":" LITERAL ;
    private void ParseMethod()
    {
        ConsumeAndAssert( ':' );

        if ( !TryParseLiteral( out method ) )
        {
            throw new InvalidOperationException( SR.EmptyMethod );
        }
    }

    private bool ParseIdentifier()
    {
        var identifier = new System.Text.StringBuilder();
        var hasEndChar = false;

        while ( !hasEndChar && NextChar() )
        {
            var c = CurrentChar;

            switch ( c )
            {
                case '.':
                case '}':
                case '=':
                    hasEndChar = true;
                    break;
                default:
                    Consume( c );
                    identifier.Append( c );
                    break;
            }
        }

        if ( identifier.Length == 0 )
        {
            return false;
        }

        CurrentVariable.FieldPath.Add( identifier.ToString() );
        return true;
    }

    private bool TryParseLiteral( [NotNullWhen( true )] out string? literal )
    {
        literal = null;

        if ( !EnsureCurrent() )
        {
            return false;
        }

        // initialize to false in case we encounter an empty literal
        var result = false;
        var builder = new StringBuilder();

        while ( true )
        {
            var c = CurrentChar;

            switch ( c )
            {
                case '/':
                case ':':
                case '}':
                    if ( !result )
                    {
                        throw new InvalidOperationException( SR.EmptyPathSegment );
                    }

                    literal = builder.ToString();
                    return result;
                default:
                    Consume( c );
                    builder.Append( c );
                    break;
            }

            result = true;

            if ( !NextChar() )
            {
                break;
            }
        }

        literal = builder.ToString();
        return result;
    }

    private void ConsumeAndAssert( char? c )
    {
        if ( !Consume( c ) )
        {
            throw new InvalidOperationException( string.Format( InvariantCulture, MissingTemplateChar, c ) );
        }
    }

    private bool Consume( char? c )
    {
        if ( !EnsureCurrent() )
        {
            return false;
        }

        if ( CurrentChar != c )
        {
            return false;
        }

        tokenStart++;
        return true;
    }

    private bool EnsureCurrent() => tokenStart < tokenEnd || NextChar();

    private bool NextChar()
    {
        if ( tokenEnd < input.Length )
        {
            tokenEnd++;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void StartVariable()
    {
        if ( inVariable )
        {
            throw new InvalidOperationException( SR.NestedVariable );
        }

        variables.Add( new HttpRouteVariable() );
        inVariable = true;
        CurrentVariable.StartSegment = segments.Count;
        CurrentVariable.HasCatchAllPath = false;
    }

    private void EndVariable()
    {
        CurrentVariable.EndSegment = segments.Count;
        inVariable = false;
    }
}