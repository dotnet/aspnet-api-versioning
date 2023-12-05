// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal sealed class SunsetLinkBuilder : ILinkBuilder
{
    private readonly SunsetPolicyBuilder policy;
    private string? language;
    private List<string>? languages;
    private string? title;
    private string? type;

    public SunsetLinkBuilder( SunsetPolicyBuilder policy, Uri linkTarget )
    {
        this.policy = policy;
        LinkTarget = linkTarget;
    }

    public Uri LinkTarget { get; }

    public ILinkBuilder Language( string value )
    {
        if ( language == null )
        {
            language = value;
        }
        else if ( languages == null )
        {
            languages = [language, value];
        }
        else
        {
            languages.Add( value );
        }

        return this;
    }

    public ILinkBuilder Link( Uri linkTarget ) => policy.Link( linkTarget );

    public ILinkBuilder Title( string value )
    {
        title = value;
        return this;
    }

    public ILinkBuilder Type( string value )
    {
        type = value;
        return this;
    }

    public LinkHeaderValue Build()
    {
        var link = new LinkHeaderValue( LinkTarget, "sunset" );

        if ( title != null )
        {
            link.Title = title;
        }

        if ( type != null )
        {
            link.Type = type;
        }

        if ( languages == null )
        {
            if ( language != null )
            {
                link.Language = language;
            }
        }
        else
        {
            for ( var i = 0; i < languages.Count; i++ )
            {
                link.Languages.Add( languages[i] );
            }
        }

        return link;
    }
}