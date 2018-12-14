namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    sealed class ApiExplorerModelConvention : IApplicationModelConvention
    {
        public void Apply( ApplicationModel application )
        {
            foreach ( var controller in application.Controllers )
            {
                if ( !controller.ControllerType.IsODataController() )
                {
                    continue;
                }

                if ( controller.ApiExplorer != null )
                {
                    controller.ApiExplorer = new ODataApiExplorerModel( controller.ApiExplorer );
                }

                foreach ( var action in controller.Actions )
                {
                    if ( action.ApiExplorer != null )
                    {
                        action.ApiExplorer = new ODataApiExplorerModel( action.ApiExplorer );
                        action.SetProperty( action.ApiExplorer );
                    }
                }
            }
        }
    }
}