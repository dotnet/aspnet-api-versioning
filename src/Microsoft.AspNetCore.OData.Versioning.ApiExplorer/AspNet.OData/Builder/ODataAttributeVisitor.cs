namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System.Collections.Generic;
    using System.Reflection;

    sealed partial class ODataAttributeVisitor
    {
        void VisitAction( ActionDescriptor action )
        {
            if ( !( action is ControllerActionDescriptor controllerAction ) )
            {
                return;
            }

            var attributes = new List<EnableQueryAttribute>( controllerAction.ControllerTypeInfo.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );

            attributes.AddRange( controllerAction.MethodInfo.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );
            VisitEnableQuery( attributes );
        }
    }
}