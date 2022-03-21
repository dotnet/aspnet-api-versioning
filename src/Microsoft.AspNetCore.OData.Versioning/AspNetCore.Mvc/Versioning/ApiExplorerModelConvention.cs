﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System.Linq;

    sealed class ApiExplorerModelConvention : IControllerModelConvention
    {
        public void Apply( ControllerModel controller )
        {
            if ( controller.ApiExplorer == null )
            {
                var model = new ApiExplorerModel();
                var attribute = controller.Attributes.OfType<ApiExplorerSettingsAttribute>().FirstOrDefault();

                if ( attribute != null )
                {
                    model.GroupName = attribute.GroupName;
                    model.IsVisible = !attribute.IgnoreApi;
                }

                controller.ApiExplorer = new ODataApiExplorerModel( model );
            }
            else if ( controller.ApiExplorer is not ODataApiExplorerModel )
            {
                controller.ApiExplorer = new ODataApiExplorerModel( controller.ApiExplorer );
            }

            for ( var j = 0; j < controller.Actions.Count; j++ )
            {
                var action = controller.Actions[j];

                if ( action.ApiExplorer == null )
                {
                    action.ApiExplorer = controller.ApiExplorer;
                }
                else if ( action.ApiExplorer is not ODataApiExplorerModel )
                {
                    action.ApiExplorer = new ODataApiExplorerModel( action.ApiExplorer );
                }

                action.SetProperty( action.ApiExplorer );
            }
        }
    }
}