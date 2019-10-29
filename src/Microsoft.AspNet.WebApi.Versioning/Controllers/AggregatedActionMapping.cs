namespace Microsoft.Web.Http.Controllers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        sealed class AggregatedActionMapping : ILookup<string, HttpActionDescriptor>
        {
            readonly IReadOnlyList<ILookup<string, HttpActionDescriptor>> actionMappings;

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
    }
}