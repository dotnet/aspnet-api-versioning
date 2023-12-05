// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.Controllers;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Template;

/// <summary>
/// Represents the <see cref="IODataControllerActionConvention">OData routing convention</see>
/// for versioned service and metadata documents.
/// </summary>
[CLSCompliant( false )]
public class VersionedMetadataRoutingConvention : MetadataRoutingConvention
{
    private static Type? metadataController;

    /// <inheritdoc />
    public override bool AppliesToController( ODataControllerActionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );
        metadataController ??= typeof( VersionedMetadataController );
        return metadataController.IsAssignableFrom( context.Controller.ControllerType );
    }

    /// <inheritdoc />
    public override bool AppliesToAction( ODataControllerActionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var action = context.Action;
        var actionName = action.ActionMethod.Name;

        if ( actionName == nameof( VersionedMetadataController.GetOptions ) )
        {
            var template = new ODataPathTemplate( MetadataSegmentTemplate.Instance );
            action.AddSelector( HttpMethod.Options.Method, context.Prefix, context.Model, template );
            return true;
        }

        return base.AppliesToAction( context );
    }
}