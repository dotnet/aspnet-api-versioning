// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an object that is the source of API version parameters.
/// </summary>
public interface IApiVersionParameterSource
{
    /// <summary>
    /// Provides API version parameter descriptions supported by the current source using the supplied context.
    /// </summary>
    /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
    void AddParameters( IApiVersionParameterDescriptionContext context );
}