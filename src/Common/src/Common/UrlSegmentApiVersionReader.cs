// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static ApiVersionParameterLocation;

/// <summary>
/// Represents an API version reader that reads the value from a path segment in the request URL.
/// </summary>
public partial class UrlSegmentApiVersionReader : IApiVersionReader
{
    private volatile bool reentrant;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlSegmentApiVersionReader"/> class.
    /// </summary>
    public UrlSegmentApiVersionReader() { }

    /// <summary>
    /// Provides API version parameter descriptions supported by the current reader using the supplied provider.
    /// </summary>
    /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
    public virtual void AddParameters( IApiVersionParameterDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );
        context.AddParameter( name: string.Empty, Path );
    }
}