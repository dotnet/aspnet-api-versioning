// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents the API version metadata applied to an endpoint.
/// </summary>
public class ApiVersionMetadata
{
    private readonly ApiVersionModel apiModel;
    private readonly ApiVersionModel endpointModel;
    private ApiVersionModel? mergedModel;
    private static ApiVersionMetadata? empty;
    private static ApiVersionMetadata? neutral;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMetadata"/> class.
    /// </summary>
    /// <param name="apiModel">The model for an entire API.</param>
    /// <param name="endpointModel">The model defined for a specific API endpoint.</param>
    /// <param name="name">The logical name of the API.</param>
    public ApiVersionMetadata( ApiVersionModel apiModel, ApiVersionModel endpointModel, string? name = default )
    {
        this.apiModel = apiModel;
        this.endpointModel = endpointModel;
        Name = name ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMetadata"/> class.
    /// </summary>
    /// <param name="other">The other <see cref="ApiVersionMetadata">instance</see> to initialize from.</param>
    protected ApiVersionMetadata( ApiVersionMetadata other )
    {
        ArgumentNullException.ThrowIfNull( other );

        apiModel = other.apiModel;
        endpointModel = other.endpointModel;
        mergedModel = other.mergedModel;
        Name = other.Name;
    }

    /// <summary>
    /// Gets an empty API version information.
    /// </summary>
    /// <value>New, empty <see cref="ApiVersionMetadata"/>.</value>
    public static ApiVersionMetadata Empty => empty ??= new( ApiVersionModel.Empty, ApiVersionModel.Empty );

    /// <summary>
    /// Gets version-neutral API version information.
    /// </summary>
    /// <value>New, version-neutral <see cref="ApiVersionMetadata"/>.</value>
    public static ApiVersionMetadata Neutral => neutral ??= new( ApiVersionModel.Neutral, ApiVersionModel.Neutral );

    /// <summary>
    /// Gets the API name.
    /// </summary>
    /// <value>The logical name of the API.</value>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the API is version-neutral.
    /// </summary>
    /// <value>True if the API is version-neutral; otherwise, false.</value>
    public bool IsApiVersionNeutral => apiModel.IsApiVersionNeutral || endpointModel.IsApiVersionNeutral;

    /// <summary>
    /// Returns an API version model for the requested mapping.
    /// </summary>
    /// <param name="mapping">One or more of the <see cref="ApiVersionMapping"/> values.</param>
    /// <returns>The mapped <see cref="ApiVersionModel">API version model</see>.</returns>
    public ApiVersionModel Map( ApiVersionMapping mapping )
    {
        switch ( mapping )
        {
            case Explicit:
                return endpointModel;
            case Implicit:
                return apiModel;
            case Explicit | Implicit:
                if ( mergedModel != null )
                {
                    return mergedModel;
                }

                if ( apiModel.IsApiVersionNeutral )
                {
                    mergedModel = apiModel;
                }
                else if ( endpointModel.IsApiVersionNeutral || endpointModel.DeclaredApiVersions.Count > 0 )
                {
                    mergedModel = endpointModel;
                }
                else
                {
                    var supported = endpointModel.SupportedApiVersions;
                    var deprecated = endpointModel.DeprecatedApiVersions;
                    var implemented = new SortedSet<ApiVersion>( supported );

                    for ( var i = 0; i < deprecated.Count; i++ )
                    {
                        implemented.Add( deprecated[i] );
                    }

                    mergedModel = new( apiModel, implemented.ToArray(), supported, deprecated );
                }

                return mergedModel;
        }

        return ApiVersionModel.Empty;
    }

    /// <summary>
    /// Returns a value indicating whether the defined metadata maps to the specified API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
    /// <returns>One of the <see cref="ApiVersionMapping"/> values.</returns>
    public ApiVersionMapping MappingTo( ApiVersion? apiVersion )
    {
        if ( endpointModel.IsApiVersionNeutral )
        {
            return Explicit;
        }

        if ( apiVersion is null )
        {
            return None;
        }

        var mappedWithImplementation = endpointModel.DeclaredApiVersions.Contains( apiVersion ) &&
                                       endpointModel.ImplementedApiVersions.Contains( apiVersion );

        if ( mappedWithImplementation )
        {
            return Explicit;
        }

        var derived = endpointModel.DeclaredApiVersions.Count == 0;

        if ( derived && apiModel.DeclaredApiVersions.Contains( apiVersion ) )
        {
            return Implicit;
        }

        return None;
    }

    /// <summary>
    /// Returns a value indicating whether the defined metadata maps to the specified API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
    /// <returns>True if the metadata explicitly or implicitly maps to the specified
    /// <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
    public bool IsMappedTo( ApiVersion? apiVersion ) => MappingTo( apiVersion ) > None;

    /// <summary>
    /// Deconstructs the metadata into its constituent parts.
    /// </summary>
    /// <param name="apiModel">The model for an entire API.</param>
    /// <param name="endpointModel">The model defined for a specific API endpoint.</param>
    public void Deconstruct( out ApiVersionModel apiModel, out ApiVersionModel endpointModel )
    {
        apiModel = this.apiModel;
        endpointModel = this.endpointModel;
    }

    /// <summary>
    /// Deconstructs the metadata into its constituent parts.
    /// </summary>
    /// <param name="apiModel">The model for an entire API.</param>
    /// <param name="endpointModel">The model defined for a specific API endpoint.</param>
    /// <param name="name">The logical name of the API.</param>
    public void Deconstruct( out ApiVersionModel apiModel, out ApiVersionModel endpointModel, out string name )
    {
        apiModel = this.apiModel;
        endpointModel = this.endpointModel;
        name = Name;
    }
}