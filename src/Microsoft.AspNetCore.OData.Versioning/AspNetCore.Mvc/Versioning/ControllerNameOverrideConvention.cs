namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;

    sealed class ControllerNameOverrideConvention : IControllerModelConvention
    {
        public void Apply( ControllerModel controller )
        {
            if ( controller.RouteValues.TryGetValue( "controller", out var name ) )
            {
                controller.ControllerName = name;
            }
        }
    }
}