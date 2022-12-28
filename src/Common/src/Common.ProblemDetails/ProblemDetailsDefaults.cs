// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides the problem details default related to API versioning.
/// </summary>
public static class ProblemDetailsDefaults
{
    private static ProblemDetailsInfo? unsupported;
    private static ProblemDetailsInfo? unspecified;
    private static ProblemDetailsInfo? invalid;
    private static ProblemDetailsInfo? ambiguous;

    /// <summary>
    /// Gets the problem details for an unsupported API version.
    /// </summary>
    public static ProblemDetailsInfo Unsupported =>
        unsupported ??= new(
            "https://docs.api-versioning.org/problems#unsupported",
            "Unsupported API version",
            "UnsupportedApiVersion" );

    /// <summary>
    /// Gets the problem details for an unspecified API version.
    /// </summary>
    public static ProblemDetailsInfo Unspecified =>
        unspecified ??= new(
            "https://docs.api-versioning.org/problems#unspecified",
            "Unspecified API version",
            "ApiVersionUnspecified" );

    /// <summary>
    /// Gets the problem details for an invalid API version.
    /// </summary>
    public static ProblemDetailsInfo Invalid =>
        invalid ??= new(
            "https://docs.api-versioning.org/problems#invalid",
            "Invalid API version",
            "InvalidApiVersion" );

    /// <summary>
    /// Gets the problem details for an ambiguous API version.
    /// </summary>
    public static ProblemDetailsInfo Ambiguous =>
        ambiguous ??= new(
            "https://docs.api-versioning.org/problems#ambiguous",
            "Ambiguous API version",
            "AmbiguousApiVersion" );

#pragma warning disable IDE0079
#pragma warning disable CA1034 // Nested types should not be visible

    /// <summary>
    /// Represents the default problem details media type.
    /// </summary>
    public static class MediaType
    {
        private const string Problem = "application/problem+";

        /// <summary>
        /// Gets the problem details media type for the JSON format.
        /// </summary>
        public const string Json = Problem + "json";

        /// <summary>
        /// Gets the problem details media type for the XML format.
        /// </summary>
        public const string Xml = Problem + "xml";
    }
}