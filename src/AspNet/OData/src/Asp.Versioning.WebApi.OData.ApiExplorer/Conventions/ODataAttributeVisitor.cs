// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNet.OData;
using System.Web.Http.Controllers;

internal sealed partial class ODataAttributeVisitor
{
    private void VisitAction( HttpActionDescriptor action )
    {
        var controller = action.ControllerDescriptor;
        var attributes = new List<EnableQueryAttribute>( controller.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );

        attributes.AddRange( action.GetCustomAttributes<EnableQueryAttribute>( inherit: true ) );
        VisitEnableQuery( attributes );
    }
}