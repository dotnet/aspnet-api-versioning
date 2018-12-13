﻿namespace Microsoft.AspNet.OData.Builder
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;

    sealed partial class ODataAttributeVisitor
    {
        void VisitAction( HttpActionDescriptor action )
        {
            Contract.Requires( action != null );

            var controller = action.ControllerDescriptor;
            var attributes = new List<EnableQueryAttribute>( controller.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );

            attributes.AddRange( action.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );
            VisitEnableQuery( attributes );
        }
    }
}