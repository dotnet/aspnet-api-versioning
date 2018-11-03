namespace Microsoft.Web.Http.Dispatcher
{
    using System;

    interface IControllerSelector
    {
        ControllerSelectionResult SelectController( ControllerSelectionContext context );
    }
}