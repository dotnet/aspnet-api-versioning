#pragma warning disable CA1812

namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using System;

    sealed class RaiseVersionedODataRoutesMapped : IStartupFilter
    {
        readonly ODataActionDescriptorChangeProvider changeProvider;

        public RaiseVersionedODataRoutesMapped( ODataActionDescriptorChangeProvider changeProvider ) =>
            this.changeProvider = changeProvider;

        public Action<IApplicationBuilder> Configure( Action<IApplicationBuilder> next )
        {
            return app =>
            {
                // execute the next configuration, which should make a call to MapVersionedODataRoute. using a IStartupFilter
                // this way will reduce the number of times we need to re-evaluate the OData action descriptions to just once
                next( app );

                // note: we don't have the required information necessary to build the odata route information
                // until one or more routes have been mapped. if anyone has subscribed changes, notify them now.
                // this might do unnecessary work, but we assume that if you're using api versioning and odata, then
                // at least one call to MapVersionedODataRoute or some other means added a route to the IODataRouteCollection
                changeProvider.NotifyChanged();
            };
        }
    }
}