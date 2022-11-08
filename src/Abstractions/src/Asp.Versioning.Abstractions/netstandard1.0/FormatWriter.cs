// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal readonly ref struct FormatWriter
{
    private readonly ApiVersionFormatProvider formatter;
    private readonly ApiVersion apiVersion;
    private readonly IFormatProvider provider;
    private readonly StringBuilder builder;

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
    }

    public void Write( in FormatToken token )
    {
        if ( token.IsLiteral )
        {
            builder.Append( token.Format );
        }
        else
        {
            formatter.AppendCustomFormat( builder, apiVersion, token.Format, provider );
        }
    }

    public void Write( char ch ) => builder.Append( ch );
}