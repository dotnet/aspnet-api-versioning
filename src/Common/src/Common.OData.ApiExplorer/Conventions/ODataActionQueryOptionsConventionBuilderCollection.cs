// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Collections;
using System.Reflection;

/// <summary>
/// Represents a collection of OData controller action query option convention builders.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class ODataActionQueryOptionsConventionBuilderCollection : IReadOnlyCollection<ODataActionQueryOptionsConventionBuilder>
{
    private readonly ODataControllerQueryOptionsConventionBuilder controllerBuilder;
    private List<ActionBuilderMapping>? actionBuilderMappings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataActionQueryOptionsConventionBuilderCollection"/> class.
    /// </summary>
    /// <param name="controllerBuilder">The associated <see cref="ODataControllerQueryOptionsConventionBuilder">controller convention builder</see>.</param>
    public ODataActionQueryOptionsConventionBuilderCollection( ODataControllerQueryOptionsConventionBuilder controllerBuilder ) =>
        this.controllerBuilder = controllerBuilder;

    /// <summary>
    /// Gets or adds a controller action convention builder for the specified method.
    /// </summary>
    /// <param name="actionMethod">The controller action method to get or add the convention builder for.</param>
    /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder">controller action convention builder</see>.</returns>
    protected internal virtual ODataActionQueryOptionsConventionBuilder GetOrAdd( MethodInfo actionMethod )
    {
        if ( actionBuilderMappings == null )
        {
            var builder = new ODataActionQueryOptionsConventionBuilder( controllerBuilder );
            actionBuilderMappings = [new( actionMethod, builder )];
            return builder;
        }

        var mapping = actionBuilderMappings.FirstOrDefault( m => m.Method == actionMethod );

        if ( mapping == null )
        {
            mapping = new( actionMethod, new( controllerBuilder ) );
            actionBuilderMappings.Add( mapping );
        }

        return mapping.Builder;
    }

    /// <summary>
    /// Gets a count of the controller action convention builders in the collection.
    /// </summary>
    /// <value>The total number of controller action convention builders in the collection.</value>
    public virtual int Count => actionBuilderMappings == null ? 0 : actionBuilderMappings.Count;

    /// <summary>
    /// Attempts to retrieve the controller action convention builder for the specified method.
    /// </summary>
    /// <param name="actionMethod">The controller action method to get the convention builder for.</param>
    /// <param name="actionBuilder">The <see cref="ODataActionQueryOptionsConventionBuilder">controller action convention builder</see> or <c>null</c>.</param>
    /// <returns>True if the <paramref name="actionBuilder">action builder</paramref> is successfully retrieved; otherwise, false.</returns>
    public virtual bool TryGetValue( MethodInfo? actionMethod, [NotNullWhen( true )] out ODataActionQueryOptionsConventionBuilder? actionBuilder )
    {
        if ( actionMethod == null || actionBuilderMappings == null || actionBuilderMappings.Count == 0 )
        {
            actionBuilder = null;
            return false;
        }

        var mapping = actionBuilderMappings.FirstOrDefault( m => m.Method == actionMethod );

        if ( mapping == null )
        {
            actionBuilder = null;
            return false;
        }

        actionBuilder = mapping.Builder;
        return true;
    }

    /// <summary>
    /// Returns an iterator that enumerates the controller action convention builders in the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> object.</returns>
    public virtual IEnumerator<ODataActionQueryOptionsConventionBuilder> GetEnumerator()
    {
        if ( actionBuilderMappings == null )
        {
            yield break;
        }

        for ( var i = 0; i < actionBuilderMappings.Count; i++ )
        {
            yield return actionBuilderMappings[i].Builder;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed partial class ActionBuilderMapping
    {
        internal ActionBuilderMapping( MethodInfo method, ODataActionQueryOptionsConventionBuilder builder )
        {
            Method = method;
            Builder = builder;
        }

        internal MethodInfo Method { get; }

        internal ODataActionQueryOptionsConventionBuilder Builder { get; }
    }
}