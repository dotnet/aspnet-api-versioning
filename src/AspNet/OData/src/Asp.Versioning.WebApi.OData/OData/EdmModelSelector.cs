// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Globalization;

/// <summary>
/// Represents an <see cref="IEdmModelSelector">EDM model selector</see>.
/// </summary>
public class EdmModelSelector : IEdmModelSelector
{
    private readonly ApiVersion maxVersion;
    private readonly IApiVersionSelector selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmModelSelector"/> class.
    /// </summary>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">models</see> to select from.</param>
    /// <param name="apiVersionSelector">The <see cref="IApiVersionSelector">selector</see> used to choose API versions.</param>
    public EdmModelSelector( IEnumerable<IEdmModel> models, IApiVersionSelector apiVersionSelector )
    {
        if ( models == null )
        {
            throw new ArgumentNullException( nameof( models ) );
        }

        selector = apiVersionSelector ?? throw new ArgumentNullException( nameof( apiVersionSelector ) );
        List<ApiVersion> versions;
        Dictionary<ApiVersion, IEdmModel> collection;

        switch ( models )
        {
            case IList<IEdmModel> list:
                versions = new( list.Count );
                collection = new( list.Count );

                for ( var i = 0; i < list.Count; i++ )
                {
                    AddVersionFromModel( list[i], versions, collection );
                }

                break;
            case IReadOnlyList<IEdmModel> list:
                versions = new( list.Count );
                collection = new( list.Count );

                for ( var i = 0; i < list.Count; i++ )
                {
                    AddVersionFromModel( list[i], versions, collection );
                }

                break;
            default:
                versions = [];
                collection = [];

                foreach ( var model in models )
                {
                    AddVersionFromModel( model, versions, collection );
                }

                break;
        }

        versions.Sort();
        maxVersion = versions.Count == 0 ? ApiVersion.Default : versions[versions.Count - 1];
        ApiVersions = versions.ToArray();
        Models = collection;
    }

    /// <inheritdoc />
    public IReadOnlyList<ApiVersion> ApiVersions { get; }

    /// <summary>
    /// Gets the collection of EDM models.
    /// </summary>
    /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="IEdmModel">EDM models</see>.</value>
    protected IDictionary<ApiVersion, IEdmModel> Models { get; }

    /// <inheritdoc />
    public virtual bool Contains( ApiVersion? apiVersion ) => apiVersion != null && Models.ContainsKey( apiVersion );

    /// <inheritdoc />
    public virtual IEdmModel? SelectModel( ApiVersion? apiVersion )
    {
        if ( apiVersion is null || Models.Count == 0 )
        {
            return default;
        }

        if ( Models.TryGetValue( apiVersion, out var model ) )
        {
            return model;
        }

        return default;
    }

    /// <inheritdoc />
    public virtual IEdmModel? SelectModel( IServiceProvider serviceProvider )
    {
        if ( Models.Count == 0 )
        {
            return default;
        }

        var request = serviceProvider.GetService<HttpRequestMessage>();

        if ( request is null )
        {
            return Models[maxVersion];
        }

        var version = request.GetRequestedApiVersion();

        if ( version is null )
        {
            var model = new ApiVersionModel( ApiVersions, Enumerable.Empty<ApiVersion>() );

            if ( ( version = selector.SelectVersion( request, model ) ) is null )
            {
                return Models[maxVersion];
            }
        }

        return Models.TryGetValue( version, out var edm ) ? edm : Models[maxVersion];
    }

    private static void AddVersionFromModel( IEdmModel model, IList<ApiVersion> versions, IDictionary<ApiVersion, IEdmModel> collection )
    {
        if ( model.GetApiVersion() is not ApiVersion version )
        {
            var message = string.Format( CultureInfo.CurrentCulture, SR.MissingAnnotation, typeof( ApiVersionAnnotation ).Name );
            throw new ArgumentException( message );
        }

        collection.Add( version, model );
        versions.Add( version );
    }
}