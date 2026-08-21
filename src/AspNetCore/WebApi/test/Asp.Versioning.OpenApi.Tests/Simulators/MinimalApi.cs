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

    /// <summary>Mixed</summary>
    /// <remarks>
    /// Text before code
    ///
    /// <code>
    ///     var index = 5;
    ///     index++;
    /// </code>
    ///
    /// Text after code
    /// </remarks>
    /// <returns>The mixed answer.</returns>
    public static int Mixed() => 42;

    /// <summary>Outlined</summary>
    /// <remarks>
    /// Text before list
    ///
    /// <list type="bullet">
    /// <item>First</item>
    /// <item>Second</item>
    /// </list>
    ///
    /// Text after list
    /// </remarks>
    /// <returns>The outlined answer.</returns>
    public static int Outlined() => 42;

    /// <summary>Stepped</summary>
    /// <remarks>
    /// Text before list
    /// <list type="number">
    ///     <item><description>First step</description></item>
    ///     <item><description>Second step</description></item>
    /// </list>
    /// Text after list
    /// </remarks>
    /// <returns>The stepped answer.</returns>
    public static int Stepped() => 42;

    /// <summary>Linked</summary>
    /// <remarks>
    /// <a href="https://example.org/spec"></a>
    ///
    /// <a href="https://example.org/spec" />
    /// </remarks>
    /// <returns>The linked answer.</returns>
    public static int Linked() => 42;

    /// <summary>
    /// Echo
    /// </summary>
    /// <param name="id">A test parameter.</param>
    /// <returns>The value of <paramref name="id"/>.</returns>
    public static int Echo( int id ) => id;
}