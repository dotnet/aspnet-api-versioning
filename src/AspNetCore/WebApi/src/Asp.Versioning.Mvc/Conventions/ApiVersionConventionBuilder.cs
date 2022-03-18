// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <content>
/// Provides additional implementation specific ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ApiVersionConventionBuilder
{
    private static bool HasDecoratedActions( ControllerModel controllerModel )
    {
        var actions = controllerModel.Actions;

        for ( var i = 0; i < actions.Count; i++ )
        {
            var action = actions[i];

            var attributes = action.Attributes;

            for ( var j = 0; j < attributes.Count; j++ )
            {
                var attribute = attributes[j];

                if ( attribute is IApiVersionProvider || attribute is IApiVersionNeutral )
                {
                    return true;
                }
            }
        }

        return false;
    }
}