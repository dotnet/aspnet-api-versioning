namespace Microsoft.AspNet.OData
{
    using System;
    using System.Reflection;

    static partial class TypeExtensions
    {
        static readonly TypeInfo ODataController = typeof( ODataController ).GetTypeInfo();
        static readonly TypeInfo MetadataController = typeof( MetadataController ).GetTypeInfo();

        internal static bool IsODataController( this Type controllerType ) => ODataController.IsAssignableFrom( controllerType );

        internal static bool IsODataController( this TypeInfo controllerType ) => ODataController.IsAssignableFrom( controllerType );

        internal static bool IsMetadataController( this TypeInfo controllerType ) => MetadataController.IsAssignableFrom( controllerType );
    }
}