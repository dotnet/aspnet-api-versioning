// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApiExplorer;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ImplicitModelBoundSettingsConvention
{
    /// <inheritdoc />
    public void ApplyTo( ApiDescription apiDescription )
    {
        ArgumentNullException.ThrowIfNull( apiDescription );

        var responses = apiDescription.SupportedResponseTypes;

        for ( var j = 0; j < responses.Count; j++ )
        {
            var response = responses[j];
            var notForSuccess = response.StatusCode < 200 || response.StatusCode >= 300;

            if ( notForSuccess )
            {
                continue;
            }

            var model = response.ModelMetadata;
            var type = model == null
                       ? response.Type
                       : model.IsEnumerableType
                         ? model.ElementType
                         : model.UnderlyingOrModelType;

            if ( type != null )
            {
                types.Add( type );
            }
        }
    }
}