// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly Lazy<IReadOnlyList<ApiVersionDescription>> apiVersionDescriptions;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see>
    /// used to enumerate the actions within an application.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public DefaultApiVersionDescriptionProvider(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        ISunsetPolicyManager sunsetPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        apiVersionDescriptions = LazyApiVersionDescriptions.Create( this, actionDescriptorCollectionProvider );
        SunsetPolicyManager = sunsetPolicyManager;
        options = apiExplorerOptions;
    }

    /// <summary>
    /// Gets the manager used to resolve sunset policies.
    /// </summary>
    /// <value>The associated <see cref="ISunsetPolicyManager">sunset policy manager</see>.</value>
    protected ISunsetPolicyManager SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
    protected ApiExplorerOptions Options => options.Value;

    /// <inheritdoc />
    public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => apiVersionDescriptions.Value;

    /// <summary>
    /// Enumerates all API versions within an application.
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see> used to enumerate the actions within an application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> EnumerateApiVersions( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
    {
        if ( actionDescriptorCollectionProvider == null )
        {
            throw new ArgumentNullException( nameof( actionDescriptorCollectionProvider ) );
        }

        var actions = actionDescriptorCollectionProvider.ActionDescriptors.Items;
        var descriptions = new List<ApiVersionDescription>( capacity: actions.Count );
        var supported = new HashSet<ApiVersion>();
        var deprecated = new HashSet<ApiVersion>();

        BucketizeApiVersions( actions, supported, deprecated );
        AppendDescriptions( descriptions, supported, deprecated: false );
        AppendDescriptions( descriptions, deprecated, deprecated: true );

        return descriptions.OrderBy( d => d.ApiVersion ).ToArray();
    }

    private void BucketizeApiVersions( IReadOnlyList<ActionDescriptor> actions, ISet<ApiVersion> supported, ISet<ApiVersion> deprecated )
    {
        var declared = new HashSet<ApiVersion>();
        var advertisedSupported = new HashSet<ApiVersion>();
        var advertisedDeprecated = new HashSet<ApiVersion>();

        for ( var i = 0; i < actions.Count; i++ )
        {
            var model = actions[i].GetApiVersionMetadata().Map( Explicit | Implicit );
            var versions = model.DeclaredApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                declared.Add( versions[j] );
            }

            versions = model.SupportedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                supported.Add( version );
                advertisedSupported.Add( version );
            }

            versions = model.DeprecatedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                deprecated.Add( version );
                advertisedDeprecated.Add( version );
            }
        }

        advertisedSupported.ExceptWith( declared );
        advertisedDeprecated.ExceptWith( declared );
        supported.ExceptWith( advertisedSupported );
        deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );

        if ( supported.Count == 0 && deprecated.Count == 0 )
        {
            supported.Add( Options.DefaultApiVersion );
        }
    }

    private void AppendDescriptions( ICollection<ApiVersionDescription> descriptions, IEnumerable<ApiVersion> versions, bool deprecated )
    {
        foreach ( var version in versions )
        {
            var groupName = version.ToString( Options.GroupNameFormat, CurrentCulture );
            var sunsetPolicy = SunsetPolicyManager.TryGetPolicy( version, out var policy ) ? policy : default;
            descriptions.Add( new( version, groupName, deprecated, sunsetPolicy ) );
        }
    }

    private sealed class LazyApiVersionDescriptions : Lazy<IReadOnlyList<ApiVersionDescription>>
    {
        private readonly DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider;
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;

        private LazyApiVersionDescriptions(
            DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            this.apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        internal static Lazy<IReadOnlyList<ApiVersionDescription>> Create(
            DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            var descriptions = new LazyApiVersionDescriptions( apiVersionDescriptionProvider, actionDescriptorCollectionProvider );
            return new( descriptions.EnumerateApiVersions );
        }

        private IReadOnlyList<ApiVersionDescription> EnumerateApiVersions() =>
            apiVersionDescriptionProvider.EnumerateApiVersions( actionDescriptorCollectionProvider );
    }
}