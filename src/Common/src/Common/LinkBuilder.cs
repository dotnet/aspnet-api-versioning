// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal abstract class LinkBuilder( Uri linkTarget, string relationType ) : ILinkBuilder
{
    private string? language;
    private List<string>? languages;
    private string? title;
    private string? type;

    protected string RelationType => relationType;

    public Uri LinkTarget => linkTarget;

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

    public abstract ILinkBuilder Link( Uri linkTarget );

    public LinkHeaderValue Build()
    {
        var link = new LinkHeaderValue( LinkTarget, RelationType );

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