// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a sunset policy builder.
/// </summary>
public interface ISunsetPolicyBuilder : IPolicyBuilder<SunsetPolicy>, IPolicyWithLink, IPolicyWithEffectiveDate
{
}