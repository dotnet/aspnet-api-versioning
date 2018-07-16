namespace Microsoft.AspNet.OData
{
    using Microsoft.AspNet.OData.Query;
    using System;
    using System.Reflection;

    static partial class TypeExtensions
    {
        static readonly TypeInfo ODataController = typeof( ODataController ).GetTypeInfo();
        static readonly TypeInfo MetadataController = typeof( MetadataController ).GetTypeInfo();
        static readonly Type ODataQueryOptions = typeof( ODataQueryOptions );
        static readonly Type DeltaType = typeof( IDelta );

        internal static bool IsODataController( this Type controllerType ) => ODataController.IsAssignableFrom( controllerType );

        internal static bool IsODataController( this TypeInfo controllerType ) => ODataController.IsAssignableFrom( controllerType );

        internal static bool IsMetadataController( this TypeInfo controllerType ) => MetadataController.IsAssignableFrom( controllerType );

        internal static bool IsODataQueryOptions( this Type type ) => ODataQueryOptions.IsAssignableFrom( type );

        internal static bool IsDelta( this Type type ) => DeltaType.IsAssignableFrom( type );
    }
}