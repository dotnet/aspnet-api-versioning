// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents and configures information about gRPC API version route parameter.
/// </summary>
public class GrpcApiVersionRouteParameter
{
    /// <summary>
    /// Gets or sets the name of the message field that represents the API version.
    /// </summary>
    /// <value>The name of the message field that represents the API version. The default value is
    /// <c>"api_version"</c>.</value>
    public string Name { get; set; } = "api_version";

    /// <summary>
    /// Gets or sets the prefix literal applied to the API version in the route template.
    /// </summary>
    /// <value>The prefix literal applied to the API version in the route template. The default value is
    /// <c>"v"</c>.</value>
    /// <remarks>
    /// <para>
    /// gRPC supports route parameters in route templates, but a parameter must match an entire segment. It
    /// cannot match part of a segment in the same manner as an ASP.NET route constraint and an API version does not
    /// include literal characters such as <c>"v"</c>. As a result, the character is not included in the route
    /// template.
    /// </para>
    /// <para>
    /// This setting adds the expected literal in the route template when it is built for the API Explorer. As an example,
    /// the gRPC route template <c>"api/{api-version}/example"</c> will be generated as
    /// <c>"api/v{api-version}/example"</c> and produce the expected behavior in the API Explorer.
    /// </para>
    /// </remarks>
    public string PrefixLiteral { get; set; } = "v";
}