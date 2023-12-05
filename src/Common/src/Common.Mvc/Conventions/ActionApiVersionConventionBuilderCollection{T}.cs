// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif

/// <summary>
/// Represents a collection of controller action convention builders.
/// </summary>
/// <typeparam name="T">The type of item the convention builder is for.</typeparam>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class ActionApiVersionConventionBuilderCollection<T> : IReadOnlyCollection<ActionApiVersionConventionBuilder<T>>
#if NETFRAMEWORK
    where T : notnull, IHttpController
#else
    where T : notnull
#endif
{
    private readonly ControllerApiVersionConventionBuilder<T> controllerBuilder;
    private List<ActionBuilderMapping>? actionBuilderMappings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderCollection{T}"/> class.
    /// </summary>
    /// <param name="controllerBuilder">The associated <see cref="ControllerApiVersionConventionBuilder{T}">controller convention builder</see>.</param>
    public ActionApiVersionConventionBuilderCollection( ControllerApiVersionConventionBuilder<T> controllerBuilder ) =>
        this.controllerBuilder = controllerBuilder;

    /// <summary>
    /// Gets or adds a controller action convention builder for the specified method.
    /// </summary>
    /// <param name="actionMethod">The controller action method to get or add the convention builder for.</param>
    /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builder</see>.</returns>
    protected internal virtual ActionApiVersionConventionBuilder<T> GetOrAdd( MethodInfo actionMethod )
    {
        ActionBuilderMapping mapping;

        if ( actionBuilderMappings is null )
        {
            mapping = new( actionMethod, new( controllerBuilder ) );
            actionBuilderMappings = [mapping];
            return mapping.Builder;
        }

        for ( var i = 0; i < actionBuilderMappings.Count; i++ )
        {
            mapping = actionBuilderMappings[i];

            if ( mapping.Method == actionMethod )
            {
                return mapping.Builder;
            }
        }

        mapping = new( actionMethod, new( controllerBuilder ) );
        actionBuilderMappings.Add( mapping );
        return mapping.Builder;
    }

    /// <inheritdoc />
    public virtual int Count => actionBuilderMappings is null ? 0 : actionBuilderMappings.Count;

    /// <summary>
    /// Attempts to retrieve the controller action convention builder for the specified method.
    /// </summary>
    /// <param name="actionMethod">The controller action method to get the convention builder for.</param>
    /// <param name="actionBuilder">The <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builder</see> or <c>null</c>.</param>
    /// <returns>True if the <paramref name="actionBuilder">action builder</paramref> is successfully retrieved; otherwise, false.</returns>
    public virtual bool TryGetValue( MethodInfo? actionMethod, [MaybeNullWhen( false )] out ActionApiVersionConventionBuilder<T> actionBuilder )
    {
        if ( actionBuilderMappings == null || actionMethod == null )
        {
            actionBuilder = default!;
            return false;
        }

        for ( var i = 0; i < actionBuilderMappings.Count; i++ )
        {
            var mapping = actionBuilderMappings[i];

            if ( mapping.Method == actionMethod )
            {
                actionBuilder = mapping.Builder;
                return true;
            }
        }

        actionBuilder = default!;
        return false;
    }

    /// <inheritdoc />
    public virtual IEnumerator<ActionApiVersionConventionBuilder<T>> GetEnumerator()
    {
        if ( actionBuilderMappings is null )
        {
            yield break;
        }

        foreach ( var mapping in actionBuilderMappings )
        {
            yield return mapping.Builder;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed partial class ActionBuilderMapping
    {
        internal ActionBuilderMapping( MethodInfo method, ActionApiVersionConventionBuilder<T> builder )
        {
            Method = method;
            Builder = builder;
        }

        internal MethodInfo Method { get; }

        internal ActionApiVersionConventionBuilder<T> Builder { get; }
    }
}