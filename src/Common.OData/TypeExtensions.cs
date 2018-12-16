namespace Microsoft.AspNet.OData
{
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods for the <see cref="Type"/> class.
    /// </summary>
    static partial class TypeExtensions
    {
        static readonly Type ODataRoutingAttributeType = typeof( ODataRoutingAttribute );
        static readonly TypeInfo MetadataController = typeof( MetadataController ).GetTypeInfo();
        static readonly Type Delta = typeof( IDelta );
        static readonly Type ODataPath = typeof( ODataPath );
        static readonly Type ODataQueryOptions = typeof( ODataQueryOptions );
        static readonly Type ODataActionParameters = typeof( ODataActionParameters );
        static readonly Type ODataParameterHelper = typeof( ODataParameterHelper );

        internal static bool IsODataController( this Type controllerType ) => Attribute.IsDefined( controllerType, ODataRoutingAttributeType );

        internal static bool IsODataController( this TypeInfo controllerType ) => Attribute.IsDefined( controllerType, ODataRoutingAttributeType );

        internal static bool IsMetadataController( this TypeInfo controllerType ) => MetadataController.IsAssignableFrom( controllerType );

        internal static bool IsODataPath( this Type type ) => ODataPath.IsAssignableFrom( type );

        internal static bool IsODataQueryOptions( this Type type ) => ODataQueryOptions.IsAssignableFrom( type );

        internal static bool IsODataActionParameters( this Type type ) => ODataActionParameters.IsAssignableFrom( type );

        internal static bool IsDelta( this Type type ) => Delta.IsAssignableFrom( type );

        internal static bool IsModelBound( this Type type ) =>
           ODataPath.IsAssignableFrom( type ) ||
           ODataQueryOptions.IsAssignableFrom( type ) ||
           Delta.IsAssignableFrom( type ) ||
           ODataActionParameters.IsAssignableFrom( type ) ||
           ODataParameterHelper.Equals( type );
    }
}