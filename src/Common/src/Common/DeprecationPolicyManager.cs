// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the default API version sunset policy manager.
/// </summary>
public partial class DeprecationPolicyManager : PolicyManager<DeprecationPolicy, DeprecationPolicyBuilder>, IDeprecationPolicyManager
{ }