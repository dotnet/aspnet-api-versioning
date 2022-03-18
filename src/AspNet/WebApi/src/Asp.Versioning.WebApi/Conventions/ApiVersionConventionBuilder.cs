// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Web.Http;
using System.Web.Http.Controllers;

/// <content>
/// Provides additional implementation specific ASP.NET Web API.
/// </content>
public partial class ApiVersionConventionBuilder
{
    private static bool HasDecoratedActions( HttpControllerDescriptor controllerDescriptor )
    {
        var actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
        var actions = actionSelector.GetActionMapping( controllerDescriptor ).SelectMany( g => g );

        foreach ( var action in actions )
        {
            if ( action.GetCustomAttributes<IApiVersionNeutral>( inherit: true ).Count > 0 )
            {
                return true;
            }

            if ( action.GetCustomAttributes<IApiVersionProvider>( inherit: false ).Count > 0 )
            {
                return true;
            }
        }

        return false;
    }
}