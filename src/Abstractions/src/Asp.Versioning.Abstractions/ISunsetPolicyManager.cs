// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an API version sunset policy manager.
/// </summary>
public interface ISunsetPolicyManager : IPolicyManager<SunsetPolicy>
{ }