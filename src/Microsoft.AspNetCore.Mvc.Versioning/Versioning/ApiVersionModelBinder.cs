﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents a model binder for an <see cref="ApiVersion">API version</see>.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public virtual Task BindModelAsync( ModelBindingContext bindingContext )
        {
            Contract.Assert( bindingContext != null );

            var feature = bindingContext.HttpContext.Features.Get<IApiVersioningFeature>();
            var model = feature.RequestedApiVersion;

            bindingContext.Result = ModelBindingResult.Success( model );

            return CompletedTask;
        }
    }
}