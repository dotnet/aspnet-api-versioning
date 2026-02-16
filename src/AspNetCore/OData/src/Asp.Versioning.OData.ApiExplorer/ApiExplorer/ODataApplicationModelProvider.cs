// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

internal sealed class ODataApplicationModelProvider : IApplicationModelProvider
{
    public int Order => 0;

    public void OnProvidersExecuted( ApplicationModelProviderContext context ) { }

    public void OnProvidersExecuting( ApplicationModelProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var application = context.Result;
        var controllers = application.Controllers;
        var odata = new ODataControllerSpecification();
        var convention = new ApiVisibilityConvention();

        for ( var i = 0; i < controllers.Count; i++ )
        {
            var controller = controllers[i];

            if ( !odata.IsSatisfiedBy( controller ) )
            {
                continue;
            }

            var actions = controller.Actions;

            for ( var j = 0; j < actions.Count; j++ )
            {
                convention.Apply( actions[j] );
            }
        }
    }
}