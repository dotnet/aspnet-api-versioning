namespace Microsoft
{
    using System;
    using System.Web.OData;

    static class TypeExtensions
    {
        internal static bool IsODataController( this Type controllerType ) => typeof( ODataController ).IsAssignableFrom( controllerType );
    }
}