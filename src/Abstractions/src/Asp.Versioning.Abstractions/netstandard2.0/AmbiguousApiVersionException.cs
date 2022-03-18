// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Runtime.Serialization;

/// <content>
/// Provides additional implementation specific to .NET Standard 2.0.
/// </content>
[Serializable]
public partial class AmbiguousApiVersionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being deserialized with.</param>
    /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being deserialized from.</param>
    protected AmbiguousApiVersionException( SerializationInfo info, StreamingContext context )
        : base( info, context ) => apiVersions = (string[]) info.GetValue( nameof( apiVersions ), typeof( string[] ) )!;

    /// <summary>
    /// Gets information about the exception being serialized.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being serialized with.</param>
    /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being serialized in.</param>
    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
        base.GetObjectData( info, context );
        info.AddValue( nameof( apiVersions ), apiVersions );
    }
}