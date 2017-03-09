namespace Microsoft.Web.Http.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    sealed class HttpControllerDescriptorComparer : IComparer<HttpControllerDescriptor>
    {
        HttpControllerDescriptorComparer() { }

        internal static IComparer<HttpControllerDescriptor> ByVersion { get; } = new HttpControllerDescriptorComparer();

        public int Compare( HttpControllerDescriptor x, HttpControllerDescriptor y )
        {
            if ( x == null )
            {
                return y == null ? 0 : -1;
            }

            if ( y == null )
            {
                return 1;
            }

            var v1 = x.GetDeclaredApiVersions().Min();
            var v2 = y.GetDeclaredApiVersions().Min();

            return v1.CompareTo( v2 );
        }
    }
}