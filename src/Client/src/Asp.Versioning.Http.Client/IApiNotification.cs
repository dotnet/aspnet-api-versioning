// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Defines the behavior of API notifications.
/// </summary>
public interface IApiNotification
{
    /// <summary>
    /// Occurs when a deprecated API is detected.
    /// </summary>
    /// <param name="context">The current notification context.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnApiDeprecatedAsync( ApiNotificationContext context, CancellationToken cancellationToken );

    /// <summary>
    /// Occurs when a newer API is detected.
    /// </summary>
    /// <param name="context">The current notification context.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnNewApiAvailableAsync( ApiNotificationContext context, CancellationToken cancellationToken );
}