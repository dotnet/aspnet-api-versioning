// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

/// <summary>
/// Represents a model binder for an <see cref="ApiVersion">API version</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersionModelBinder : IModelBinder
{
    /// <inheritdoc />
    public virtual Task BindModelAsync( ModelBindingContext bindingContext )
    {
        ArgumentNullException.ThrowIfNull( bindingContext );

        var feature = bindingContext.HttpContext.ApiVersioningFeature();
        var model = feature.RequestedApiVersion;

        if ( model != null )
        {
            bindingContext.ValidationState.Add( model, new ValidationStateEntry() { SuppressValidation = true } );
        }

        bindingContext.Result = ModelBindingResult.Success( model );

        return Task.CompletedTask;
    }
}