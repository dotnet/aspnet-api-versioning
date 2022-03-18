// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Collections;
using System.Web.Http.Controllers;

internal sealed class AggregatedActionMapping : ILookup<string, HttpActionDescriptor>
{
    private readonly IReadOnlyList<ILookup<string, HttpActionDescriptor>> actionMappings;

    internal AggregatedActionMapping( IReadOnlyList<ILookup<string, HttpActionDescriptor>> actionMappings ) =>
        this.actionMappings = actionMappings;

    public IEnumerable<HttpActionDescriptor> this[string key] =>
        actionMappings.Where( am => am.Contains( key ) ).SelectMany( am => am[key] );

    public int Count => actionMappings.Aggregate( 0, ( count, mappings ) => count + mappings.Count );

    public bool Contains( string key ) => actionMappings.Any( am => am.Contains( key ) );

    public IEnumerator<IGrouping<string, HttpActionDescriptor>> GetEnumerator() =>
        actionMappings.SelectMany( am => am ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}