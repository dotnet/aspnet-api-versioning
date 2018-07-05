namespace Microsoft.AspNet.OData
{
    using System;

    static class TypeExtensions
    {
        internal static bool IsODataController( this Type controllerType ) => typeof( ODataController ).IsAssignableFrom( controllerType );
    }
}