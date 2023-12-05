// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Represents an API version metadata collection provider for controller actions.
/// </summary>
[CLSCompliant( false )]
public sealed class ActionApiVersionMetadataCollationProvider : IApiVersionMetadataCollationProvider
{
    private readonly IActionDescriptorCollectionProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionMetadataCollationProvider"/> class.
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The underlying
    /// <see cref="IActionDescriptorCollectionProvider">action descriptor collection provider</see>.</param>
    public ActionApiVersionMetadataCollationProvider( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider ) =>
        provider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException( nameof( actionDescriptorCollectionProvider ) );

    /// <inheritdoc />
    public int Version => provider.ActionDescriptors.Version;

    /// <inheritdoc />
    public void Execute( ApiVersionMetadataCollationContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var actions = provider.ActionDescriptors.Items;

        for ( var i = 0; i < actions.Count; i++ )
        {
            var action = actions[i];
            var item = action.GetApiVersionMetadata();
            var groupName = GetGroupName( action );

            context.Results.Add( item, groupName );
        }
    }

    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs
    private static string? GetGroupName( ActionDescriptor action )
    {
        var endpointGroupName = action.EndpointMetadata.OfType<IEndpointGroupNameMetadata>().LastOrDefault();

        if ( endpointGroupName is null )
        {
            return action.GetProperty<ApiDescriptionActionData>()?.GroupName;
        }

        return endpointGroupName.EndpointGroupName;
    }
}