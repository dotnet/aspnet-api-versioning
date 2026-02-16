// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a deprecation policy builder.
/// </summary>
public interface IDeprecationPolicyBuilder : IPolicyBuilder<DeprecationPolicy>, IPolicyWithLink, IPolicyWithEffectiveDate
{
}