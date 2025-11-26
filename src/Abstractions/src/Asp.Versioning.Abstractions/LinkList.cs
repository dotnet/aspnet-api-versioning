// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Collections.ObjectModel;

internal abstract class LinkList : Collection<LinkHeaderValue>
{
    public LinkList() { }

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

    protected abstract void EnsureRelationType( LinkHeaderValue item );
}