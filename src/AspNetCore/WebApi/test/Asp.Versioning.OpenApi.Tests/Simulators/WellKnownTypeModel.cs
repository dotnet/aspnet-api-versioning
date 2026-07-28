// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

using Google.Protobuf.WellKnownTypes;

/// <summary>
/// Represents a model composed of well-known Protocol Buffers types.
/// </summary>
/// <remarks>
/// The types are used directly rather than through a generated message so that the transformer can be
/// exercised without gRPC. The schemas are generated from the CLR types by System.Text.Json either way.
/// </remarks>
public class WellKnownTypeModel
{
    /// <summary>
    /// Gets or sets when the model was created.
    /// </summary>
    public Timestamp CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets how long the model remains valid.
    /// </summary>
    public Duration ValidFor { get; set; }

    /// <summary>
    /// Gets or sets the fields to update.
    /// </summary>
    public FieldMask UpdateMask { get; set; }

    /// <summary>
    /// Gets or sets additional details about the model.
    /// </summary>
    public Any Details { get; set; }
}