﻿namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using Microsoft.AspNet.OData;
    using System;

    /// <summary>
    /// Represents a specification that matches API controllers if they use the OData protocol.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ODataControllerSpecification : IApiControllerSpecification
    {
        /// <inheritdoc />
        public bool IsSatisfiedBy( ControllerModel controller )
        {
            Arg.NotNull( controller, nameof( controller ) );
            return controller.ControllerType.IsODataController();
        }
    }
}