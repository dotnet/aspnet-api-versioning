// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents a user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the username associated with the account.
    /// </summary>
    /// <example>John Doe</example>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; }
}