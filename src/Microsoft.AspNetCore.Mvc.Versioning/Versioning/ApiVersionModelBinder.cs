namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using System;
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
            if ( bindingContext == null )
            {
                throw new ArgumentNullException( nameof( bindingContext ) );
            }

            var feature = bindingContext.HttpContext.Features.Get<IApiVersioningFeature>();
            var model = feature.RequestedApiVersion;

            if ( model != null )
            {
                bindingContext.ValidationState.Add( model, new ValidationStateEntry() { SuppressValidation = true } );
            }

            bindingContext.Result = ModelBindingResult.Success( model );

            return CompletedTask;
        }
    }
}