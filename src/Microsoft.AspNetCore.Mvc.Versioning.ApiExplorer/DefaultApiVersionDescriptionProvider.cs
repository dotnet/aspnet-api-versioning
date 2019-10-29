namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.Globalization.CultureInfo;

    /// <summary>
    /// Represents the default implementation of an object that discovers and describes the API version information within an application.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
    {
        readonly Lazy<IReadOnlyList<ApiVersionDescription>> apiVersionDescriptions;
        readonly IOptions<ApiExplorerOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see> used to enumerate the actions within an application.</param>
        /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured <see cref="ApiExplorerOptions">API explorer options</see>.</param>
        public DefaultApiVersionDescriptionProvider( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IOptions<ApiExplorerOptions> apiExplorerOptions )
        {
            apiVersionDescriptions = LazyApiVersionDescriptions.Create( this, actionDescriptorCollectionProvider );
            options = apiExplorerOptions;
        }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ApiExplorerOptions Options => options.Value;

        /// <summary>
        /// Gets a read-only list of discovered API version descriptions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</value>
        public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => apiVersionDescriptions.Value;

        /// <summary>
        /// Determines whether the specified action is deprecated for the provided API version.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the specified <paramref name="actionDescriptor">action</paramref> is deprecated for the
        /// <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
        public virtual bool IsDeprecated( ActionDescriptor actionDescriptor, ApiVersion apiVersion )
        {
            var model = actionDescriptor.GetApiVersionModel();
            return !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion );
        }

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

            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();
            var descriptions = new List<ApiVersionDescription>();

            BucketizeApiVersions( actionDescriptorCollectionProvider.ActionDescriptors.Items, supported, deprecated );
            AppendDescriptions( descriptions, supported, deprecated: false );
            AppendDescriptions( descriptions, deprecated, deprecated: true );

            return descriptions.OrderBy( d => d.ApiVersion ).ToArray();
        }

        void BucketizeApiVersions( IReadOnlyList<ActionDescriptor> actions, ISet<ApiVersion> supported, ISet<ApiVersion> deprecated )
        {
            var declared = new HashSet<ApiVersion>();
            var advertisedSupported = new HashSet<ApiVersion>();
            var advertisedDeprecated = new HashSet<ApiVersion>();

            foreach ( var action in actions )
            {
                var model = action.GetApiVersionModel( Explicit | Implicit );

                foreach ( var version in model.DeclaredApiVersions )
                {
                    declared.Add( version );
                }

                foreach ( var version in model.SupportedApiVersions )
                {
                    supported.Add( version );
                    advertisedSupported.Add( version );
                }

                foreach ( var version in model.DeprecatedApiVersions )
                {
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

        void AppendDescriptions( ICollection<ApiVersionDescription> descriptions, IEnumerable<ApiVersion> versions, bool deprecated )
        {
            foreach ( var version in versions )
            {
                var groupName = version.ToString( Options.GroupNameFormat, CurrentCulture );
                descriptions.Add( new ApiVersionDescription( version, groupName, deprecated ) );
            }
        }

        sealed class LazyApiVersionDescriptions : Lazy<IReadOnlyList<ApiVersionDescription>>
        {
            readonly DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider;
            readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;

            LazyApiVersionDescriptions( DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
            {
                this.apiVersionDescriptionProvider = apiVersionDescriptionProvider;
                this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            }

            internal static Lazy<IReadOnlyList<ApiVersionDescription>> Create( DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
            {
                var descriptions = new LazyApiVersionDescriptions( apiVersionDescriptionProvider, actionDescriptorCollectionProvider );
                return new Lazy<IReadOnlyList<ApiVersionDescription>>( descriptions.EnumerateApiVersions );
            }

            IReadOnlyList<ApiVersionDescription> EnumerateApiVersions() => apiVersionDescriptionProvider.EnumerateApiVersions( actionDescriptorCollectionProvider );
        }
    }
}