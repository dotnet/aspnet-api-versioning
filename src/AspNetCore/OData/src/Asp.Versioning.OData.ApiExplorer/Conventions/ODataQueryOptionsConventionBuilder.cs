// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ODataQueryOptionsConventionBuilder
{
    private static Type GetController( ApiDescription apiDescription )
    {
        if ( apiDescription.ActionDescriptor is ControllerActionDescriptor action )
        {
            return action.ControllerTypeInfo;
        }

        return typeof( object );
    }
}