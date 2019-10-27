namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System;
    using System.Reflection;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ODataQueryOptionsConventionBuilder
    {
        static TypeInfo GetKey( Type type ) => type.GetTypeInfo();

        static TypeInfo GetController( ApiDescription apiDescription )
        {
            if ( apiDescription.ActionDescriptor is ControllerActionDescriptor action )
            {
                return action.ControllerTypeInfo;
            }

            return typeof( object ).GetTypeInfo();
        }

        static bool IsODataLike( ApiDescription description )
        {
            if ( description.ActionDescriptor is ControllerActionDescriptor action )
            {
                return Attribute.IsDefined( action.ControllerTypeInfo, typeof( EnableQueryAttribute ), inherit: true );
            }

            return false;
        }
    }
}