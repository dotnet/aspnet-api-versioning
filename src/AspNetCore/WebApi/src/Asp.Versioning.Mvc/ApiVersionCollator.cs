// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Runtime.CompilerServices;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents an object that collates <see cref="ApiVersion">API versions</see> per <see cref="ActionDescriptor">action</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersionCollator : IActionDescriptorProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionCollator"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    public ApiVersionCollator( IControllerNameConvention namingConvention ) => NamingConvention = namingConvention;

    /// <summary>
    /// Gets the controller naming convention associated with the collator.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    protected IControllerNameConvention NamingConvention { get; }

    /// <inheritdoc />
    public int Order { get; protected set; }

    /// <inheritdoc />
    public virtual void OnProvidersExecuted( ActionDescriptorProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        foreach ( var actions in GroupActionsByController( context.Results ) )
        {
            var collatedModel = CollateModel( actions );

            for ( var i = 0; i < actions.Count; i++ )
            {
                var action = actions[i];
                var metadata = action.GetApiVersionMetadata();

                if ( metadata.IsApiVersionNeutral )
                {
                    continue;
                }

                var (apiModel, endpointModel, name) = metadata;

                metadata = new( apiModel, endpointModel.Aggregate( collatedModel ), name );
                action.AddOrReplaceApiVersionMetadata( metadata );
            }
        }
    }

    /// <inheritdoc />
    public virtual void OnProvidersExecuting( ActionDescriptorProviderContext context ) { }

    /// <summary>
    /// Resolves and returns the logical controller name for the specified action.
    /// </summary>
    /// <param name="action">The <see cref="ActionDescriptor">action</see> to get the controller name from.</param>
    /// <returns>The logical name of the associated controller.</returns>
    /// <remarks>
    /// <para>
    /// The logical controller name is used to collate actions together and aggregate API versions. The
    /// default implementation uses the "controller" route parameter and falls back to the
    /// <see cref="ControllerActionDescriptor.ControllerName"/> property when available.
    /// </para>
    /// <para>
    /// The default implementation will also trim trailing numbers in the controller name by convention. For example,
    /// the type "Values2Controller" will have the controller name "Values2", which will be trimmed to just "Values".
    /// This behavior can be changed by using the <see cref="ControllerNameAttribute"/> or overriding the default
    /// implementation.
    /// </para>
    /// </remarks>
    protected virtual string GetControllerName( ActionDescriptor action )
    {
        ArgumentNullException.ThrowIfNull( action );

        if ( !action.RouteValues.TryGetValue( "controller", out var name ) || name is null )
        {
            name = action is ControllerActionDescriptor controllerAction ? controllerAction.ControllerName : string.Empty;
        }

        return NamingConvention.GroupName( name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsUnversioned( ActionDescriptor action ) => action.GetApiVersionMetadata() == ApiVersionMetadata.Empty;

    private IEnumerable<IReadOnlyList<ActionDescriptor>> GroupActionsByController( IList<ActionDescriptor> actions )
    {
        var groups = new Dictionary<string, List<ActionDescriptor>>( StringComparer.OrdinalIgnoreCase );

        for ( var i = 0; i < actions.Count; i++ )
        {
            var action = actions[i];

            if ( IsUnversioned( action ) )
            {
                continue;
            }

            var key = GetControllerName( action );

            if ( string.IsNullOrEmpty( key ) )
            {
                continue;
            }

            if ( !groups.TryGetValue( key, out var values ) )
            {
                groups.Add( key, values = [] );
            }

            values.Add( action );
        }

        foreach ( var value in groups.Values )
        {
            yield return value;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ApiVersionModel CollateModel( IEnumerable<ActionDescriptor> actions ) =>
        actions.Select( a => a.GetApiVersionMetadata().Map( Explicit ) ).Aggregate();
}