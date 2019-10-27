namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Web.Http.Description;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ODataQueryOptionsConventionBuilder
    {
        static Type GetKey( Type type ) => type;

        static Type GetController( ApiDescription apiDescription ) =>
            apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;

        static bool IsODataLike( ApiDescription description ) =>
            description.ActionDescriptor.GetCustomAttributes<EnableQueryAttribute>( inherit: true ).Count > 0;
    }
}