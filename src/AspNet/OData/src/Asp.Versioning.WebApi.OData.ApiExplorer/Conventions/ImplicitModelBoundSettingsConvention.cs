// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.OData;
using System.Web.Http.Description;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class ImplicitModelBoundSettingsConvention : IModelConfiguration, IODataQueryOptionsConvention
{
    /// <inheritdoc />
    public void ApplyTo( ApiDescription apiDescription )
    {
        var response = apiDescription.ResponseDescription;
        var type = response.ResponseType ?? response.DeclaredType;

        if ( type == null )
        {
            return;
        }

        if ( type.IsEnumerable( out var itemType ) )
        {
            type = itemType;
        }

        types.Add( type! );
    }
}