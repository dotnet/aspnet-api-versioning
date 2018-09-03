namespace System.Web.Http.Controllers
{
    using System;
    using System.Collections.Generic;

    static class HttpControllerDescriptorExtensions
    {
        internal static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor )
        {
            if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
            {
                foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                {
                    yield return groupedControllerDescriptor;
                }
            }
            else
            {
                yield return controllerDescriptor;
            }
        }
    }
}