// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.ComponentModel;
using System.Runtime.Serialization;

/// <content>
/// Provides additional implementation specific to .NET Standard 2.0.
/// </content>
[Serializable]
public partial class AmbiguousApiVersionException : Exception
{
    private const string LegacyFormatterImplMessage = "This API supports obsolete formatter-based serialization. It should not be called or extended by application code.";
#if NET
    private const string LegacyFormatterImplDiagId = "SYSLIB0051";
    private const string SharedUrlFormat = "https://aka.ms/dotnet-warnings/{0}";
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being deserialized with.</param>
    /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being deserialized from.</param>
#if NET
    [Obsolete( LegacyFormatterImplMessage, DiagnosticId = LegacyFormatterImplDiagId, UrlFormat = SharedUrlFormat )]
#else
    [Obsolete( LegacyFormatterImplMessage )]
#endif
    [EditorBrowsable( EditorBrowsableState.Never )]
    protected AmbiguousApiVersionException( SerializationInfo info, StreamingContext context )
        : base( info, context ) => apiVersions = (string[]) info.GetValue( nameof( apiVersions ), typeof( string[] ) )!;

    /// <summary>
    /// Gets information about the exception being serialized.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being serialized with.</param>
    /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being serialized in.</param>
#if NET
    [Obsolete( LegacyFormatterImplMessage, DiagnosticId = LegacyFormatterImplDiagId, UrlFormat = SharedUrlFormat )]
#endif
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
        base.GetObjectData( info, context );
        info.AddValue( nameof( apiVersions ), apiVersions );
    }
}