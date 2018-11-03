namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;

    /// <summary>
    /// Defines the behavior of a convention builder for a controller.
    /// </summary>
    [CLSCompliant( false )]
    public interface IControllerConventionBuilder : IApiVersionConventionBuilder, IApiVersionConvention<ControllerModel>
    {
    }
}