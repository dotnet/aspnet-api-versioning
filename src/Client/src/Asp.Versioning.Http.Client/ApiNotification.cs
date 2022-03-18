// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

#if NETSTANDARD2_0 || !NETSTANDARD
using static System.Threading.Tasks.Task;
#endif

/// <summary>
/// Represents the base implementation for an <see cref="IApiNotification">API notification</see>.
/// </summary>
public abstract class ApiNotification : IApiNotification
{
#if NETSTANDARD1_1
    private static readonly Task CompletedTask = Task.FromResult( default( object ) );
#endif
    private static IApiNotification? none;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiNotification"/> class.
    /// </summary>
    protected ApiNotification() { }

    /// <summary>
    /// Gets a value representing no API notification.
    /// </summary>
    /// <value>An <see cref="IApiNotification">API notification</see> which performs no action.</value>
    public static IApiNotification None => none ??= new NoApiNotification();

    /// <summary>
    /// Occurs when a deprecated API is detected.
    /// </summary>
    /// <param name="context">The current notification context.</param>
    protected virtual void OnApiDeprecated( ApiNotificationContext context ) { }

    /// <summary>
    /// Occurs when a newer API is detected.
    /// </summary>
    /// <param name="context">The current notification context.</param>
    protected virtual void OnNewApiAvailable( ApiNotificationContext context ) { }

    /// <inheritdoc />
    public virtual Task OnApiDeprecatedAsync( ApiNotificationContext context, CancellationToken cancellationToken )
    {
        OnApiDeprecated( context );
        return CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnNewApiAvailableAsync( ApiNotificationContext context, CancellationToken cancellationToken )
    {
        OnNewApiAvailable( context );
        return CompletedTask;
    }

    private sealed class NoApiNotification : ApiNotification
    {
        internal NoApiNotification() { }
    }
}