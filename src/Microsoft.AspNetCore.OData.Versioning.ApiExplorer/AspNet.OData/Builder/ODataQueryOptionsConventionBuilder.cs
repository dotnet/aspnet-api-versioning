namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    }
}