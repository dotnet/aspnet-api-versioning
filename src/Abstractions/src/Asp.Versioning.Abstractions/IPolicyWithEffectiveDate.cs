// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a policy which can be configured to only be effective after a particular date.
/// </summary>
public interface IPolicyWithEffectiveDate
{
    /// <summary>
    /// Sets the effective date when a policy is applied.
    /// </summary>
    /// <param name="effectiveDate">The <see cref="DateTimeOffset">date and time</see> when a policy is applied.</param>
    void SetEffectiveDate( DateTimeOffset effectiveDate );
}