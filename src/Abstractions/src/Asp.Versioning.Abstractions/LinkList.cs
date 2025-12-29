// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Collections.ObjectModel;
using System.Globalization;

internal sealed class LinkList( string relationType ) : Collection<LinkHeaderValue>
{
    private readonly string relationType = relationType;

    protected override void InsertItem( int index, LinkHeaderValue item )
    {
        EnsureRelationType( item );
        base.InsertItem( index, item );
    }

    protected override void SetItem( int index, LinkHeaderValue item )
    {
        EnsureRelationType( item );
        base.SetItem( index, item );
    }

    private void EnsureRelationType( LinkHeaderValue item )
    {
        if ( !item.RelationType.Equals( relationType, StringComparison.OrdinalIgnoreCase ) )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidRelationType, relationType );
            throw new ArgumentException( message, nameof( item ) );
        }
    }
}