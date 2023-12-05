// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class VersionedODataModelBuilder
{
    private readonly IODataApiVersionCollectionProvider apiVersionCollectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
    /// </summary>
    /// <param name="apiVersionCollectionProvider">The <see cref="IODataApiVersionCollectionProvider">provider</see> for OData-specific API versions.</param>
    /// <param name="modelConfigurations">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IModelConfiguration">model configurations</see> associated with the model builder.</param>
    public VersionedODataModelBuilder(
        IODataApiVersionCollectionProvider apiVersionCollectionProvider,
        IEnumerable<IModelConfiguration> modelConfigurations )
    {
        ArgumentNullException.ThrowIfNull( apiVersionCollectionProvider );
        ArgumentNullException.ThrowIfNull( modelConfigurations );

        this.apiVersionCollectionProvider = apiVersionCollectionProvider;

        foreach ( var configuration in modelConfigurations )
        {
            ModelConfigurations.Add( configuration );
        }
    }

    /// <summary>
    /// Gets the API versions for all known OData routes.
    /// </summary>
    /// <returns>The <see cref="IReadOnlyList{T}">sequence</see> of <see cref="ApiVersion">API versions</see>
    /// for all known OData routes.</returns>
    protected virtual IReadOnlyList<ApiVersion> GetApiVersions() => apiVersionCollectionProvider.ApiVersions;
}