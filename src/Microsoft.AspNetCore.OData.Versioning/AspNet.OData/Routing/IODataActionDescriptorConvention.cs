namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System;

    internal interface IODataActionDescriptorConvention
    {
        void Apply( ActionDescriptorProviderContext context, ControllerActionDescriptor action );
    }
}