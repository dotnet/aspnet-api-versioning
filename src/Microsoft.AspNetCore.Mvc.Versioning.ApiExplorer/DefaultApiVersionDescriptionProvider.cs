namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents the default implementation of an object that discovers and describes the API version information within an application.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
    {
        readonly Lazy<IReadOnlyList<ApiVersionDescription>> apiVersionDescriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see> used to enumerate the actions within an application.</param>
        /// <param name="groupNameFormatter">The <see cref="IApiVersionGroupNameFormatter">formatter</see> used to get group names for API versions.</param>
        public DefaultApiVersionDescriptionProvider( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IApiVersionGroupNameFormatter groupNameFormatter )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( groupNameFormatter, nameof( groupNameFormatter ) );

            apiVersionDescriptions = new Lazy<IReadOnlyList<ApiVersionDescription>>( () => EnumerateApiVersions( actionDescriptorCollectionProvider ) );
            GroupNameFormatter = groupNameFormatter;
        }

        /// <summary>
        /// Gets the group name formatter associated with the provider.
        /// </summary>
        /// <value>The <see cref="IApiVersionGroupNameFormatter">group name formatter</see> used to format group names.</value>
        protected IApiVersionGroupNameFormatter GroupNameFormatter { get; }

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
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var model = actionDescriptor.GetProperty<ApiVersionModel>();

            if ( model != null && !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion ) )
            {
                return true;
            }

            model = actionDescriptor.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

            return model != null && !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion );
        }

        /// <summary>
        /// Enumerates all API versions within an application.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see> used to enumerate the actions within an application.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</returns>
        protected virtual IReadOnlyList<ApiVersionDescription> EnumerateApiVersions( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersionDescription>>() != null );

            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();
            var descriptions = new List<ApiVersionDescription>();

            BucketizeApiVersions( actionDescriptorCollectionProvider.ActionDescriptors.Items, supported, deprecated );
            AppendDescriptions( descriptions, supported, deprecated: false );
            AppendDescriptions( descriptions, deprecated, deprecated: true );

            return descriptions.OrderBy( d => d.ApiVersion ).ToArray();
        }

        static void BucketizeApiVersions( IReadOnlyList<ActionDescriptor> actions, ISet<ApiVersion> supported, ISet<ApiVersion> deprecated )
        {
            Contract.Requires( actions != null );
            Contract.Requires( supported != null );
            Contract.Requires( deprecated != null );

            var declared = new HashSet<ApiVersion>();
            var advertisedSupported = new HashSet<ApiVersion>();
            var advertisedDeprecated = new HashSet<ApiVersion>();

            foreach ( var action in actions )
            {
                var model = action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;
                var implicitModel = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

                foreach ( var version in model.DeclaredApiVersions.Union( implicitModel.DeclaredApiVersions ) )
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
        }

        void AppendDescriptions( ICollection<ApiVersionDescription> descriptions, IEnumerable<ApiVersion> versions, bool deprecated )
        {
            Contract.Requires( descriptions != null );
            Contract.Requires( versions != null );

            foreach ( var version in versions )
            {
                var groupName = GroupNameFormatter.GetGroupName( version );
                descriptions.Add( new ApiVersionDescription( version, groupName, deprecated ) );
            }
        }
    }
}