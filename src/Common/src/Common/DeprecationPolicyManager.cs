// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the default API version deprecation policy manager.
/// </summary>
/// <remarks>
/// This class serves as a type alias to hide the generic arguments of <see cref="PolicyManager{TPolicy, TPolicyBuilder}"/>.
/// </remarks>
public partial class DeprecationPolicyManager : PolicyManager<DeprecationPolicy, IDeprecationPolicyBuilder>
{ }