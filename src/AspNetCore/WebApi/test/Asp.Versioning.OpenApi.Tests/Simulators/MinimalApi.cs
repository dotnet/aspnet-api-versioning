// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

public static class MinimalApi
{
    /// <summary>
    /// Test
    /// </summary>
    /// <description>A test API.</description>
    /// <param name="id" example="42">A test parameter.</param>
    /// <returns>The original identifier.</returns>
    /// <response code="200">Pass</response>
    /// <response code="400">Fail</response>
    public static int Get( int id ) => id;

    /// <summary>
    /// One
    /// </summary>
    /// <returns>The only answer.</returns>
    public static int One() => 42;

    /// <summary>
    /// Many
    /// </summary>
    /// <returns>An ambiguous answer.</returns>
    public static int Many() => 42;

    /// <summary>
    /// Detailed
    /// </summary>
    /// <remarks>The long-form explanation.</remarks>
    /// <description>The short-form explanation.</description>
    /// <returns>The detailed answer.</returns>
    public static int Detailed() => 42;

    /// <summary>
    /// Echo
    /// </summary>
    /// <param name="id">A test parameter.</param>
    /// <returns>The value of <paramref name="id"/>.</returns>
    public static int Echo( int id ) => id;
}