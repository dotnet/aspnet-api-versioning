// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

public static class MinimalApi
{
    /// <summary>
    /// Test
    /// </summary>
    /// <description>A test API.</description>
    /// <param name="id">A test parameter.</param>
    /// <returns>The original identifier.</returns>
    /// <response code="200">Pass</response>
    /// <response code="400">Fail</response>
    public static int Get( int id ) => id;
}