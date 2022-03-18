// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Query;
using System.Reflection;

internal sealed partial class ODataAttributeVisitor
{
    private void VisitAction( ActionDescriptor action )
    {
        if ( action is not ControllerActionDescriptor controllerAction )
        {
            return;
        }

        var controllerAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<EnableQueryAttribute>( inherit: true );
        var actionAttributes = controllerAction.MethodInfo.GetCustomAttributes<EnableQueryAttribute>( inherit: true );
        var attributes = controllerAttributes.Concat( actionAttributes ).ToArray();

        VisitEnableQuery( attributes );
    }
}