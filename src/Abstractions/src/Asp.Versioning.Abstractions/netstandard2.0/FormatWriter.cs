// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal ref struct FormatWriter
{
    private readonly ApiVersionFormatProvider formatter;
    private readonly ApiVersion apiVersion;
    private readonly IFormatProvider provider;
    private readonly StringBuilder? builder;
    private Span<char> text;
    private int totalWritten;

    internal FormatWriter(
        ApiVersionFormatProvider formatter,
        StringBuilder builder,
        ApiVersion apiVersion,
        IFormatProvider provider )
    {
        this.formatter = formatter;
        this.builder = builder;
        this.apiVersion = apiVersion;
        this.provider = provider;
        text = default;
        totalWritten = 0;
        Succeeded = true;
    }

    internal FormatWriter(
        ApiVersionFormatProvider formatter,
        Span<char> text,
        ApiVersion apiVersion,
        IFormatProvider provider )
    {
        this.formatter = formatter;
        builder = default;
        this.text = text;
        this.apiVersion = apiVersion;
        this.provider = provider;
        totalWritten = 0;
        Succeeded = true;
    }

    public bool Succeeded { get; private set; }

    public readonly int Written => totalWritten;

    public void Write( in FormatToken token )
    {
        if ( builder is null )
        {
            if ( !Succeeded )
            {
                return;
            }

            if ( token.IsLiteral )
            {
                var length = token.Format.Length;

                if ( text.Length >= length )
                {
                    token.Format.CopyTo( text );
                    totalWritten += length;
                    text = Str.Substring( text, length );
                }
                else
                {
                    Succeeded = false;
                }
            }
            else
            {
                if ( formatter.TryAppendCustomFormat( text, out var written, apiVersion, token.Format, provider ) )
                {
                    totalWritten += written;
                    text = Str.Substring( text, written );
                }
                else
                {
                    Succeeded = false;
                }
            }
        }
        else
        {
            if ( token.IsLiteral )
            {
                builder.Append( Str.StringOrSpan( token.Format ) );
            }
            else
            {
                formatter.AppendCustomFormat( builder, apiVersion, token.Format, provider );
            }
        }
    }

    public void Write( char ch )
    {
        if ( builder is null )
        {
            if ( text.Length > 0 )
            {
                text[0] = ch;
                text = Str.Substring( text, 1 );
                totalWritten++;
            }
            else
            {
                Succeeded = false;
            }
        }
        else
        {
            builder.Append( ch );
        }
    }
}