namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of an API controller filter.
    /// </summary>
    [CLSCompliant( false )]
    public interface IApiControllerFilter
    {
        /// <summary>
        /// Applies the filter to the provided list of controllers.
        /// </summary>
        /// <param name="controllers">The <see cref="IList{T}">list</see> of
        /// <see cref="ControllerModel">controllers</see> to filter.</param>
        /// <returns>A new, filtered <see cref="IList{T}">list</see> of API
        /// <see cref="ControllerModel">controllers</see>.</returns>
        IList<ControllerModel> Apply( IList<ControllerModel> controllers );
    }
}