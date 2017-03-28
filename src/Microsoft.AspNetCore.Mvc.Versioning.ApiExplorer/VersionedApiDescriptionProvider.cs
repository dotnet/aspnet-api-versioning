namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents an API explorer that provides <see cref="ApiDescription">API descriptions</see> for actions represented by
    /// <see cref="ControllerActionDescriptor">controller action descriptors</see> that are <see cref="ApiVersion">API version</see> aware.
    /// </summary>
    [CLSCompliant( false )]
    public class VersionedApiDescriptionProvider : IApiDescriptionProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="VersionedApiDescriptionProvider"/> class.
        /// </summary>
        /// <param name="groupNameFormatter">The <see cref="IApiVersionGroupNameFormatter">formatter</see> used to get group names for API versions.</param>
        public VersionedApiDescriptionProvider( IApiVersionGroupNameFormatter groupNameFormatter, IModelMetadataProvider metadadataProvider )
        {
            Arg.NotNull( groupNameFormatter, nameof( groupNameFormatter ) );
            GroupNameFormatter = groupNameFormatter;

            this.metadadataProvider = metadadataProvider;
        }

        readonly IModelMetadataProvider metadadataProvider;


        /// <summary>
        /// Gets the order prescendence of the current API description provider.
        /// </summary>
        /// <value>The order prescendence of the current API description provider. The default value is 0.</value>
        public virtual int Order => 0;

        /// <summary>
        /// Gets the group name formatter associated with the provider.
        /// </summary>
        /// <value>The <see cref="IApiVersionGroupNameFormatter">group name formatter</see> used to format group names.</value>
        protected IApiVersionGroupNameFormatter GroupNameFormatter { get; }

        /// <summary>
        /// Determines whether the specified action should be explored.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> for action being explored.</param>
        /// <returns>True if the action should be explored; otherwise, false.</returns>
        protected virtual bool ShouldExploreAction( ActionDescriptor actionDescriptor, ApiVersion apiVersion )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Arg.NotNull( apiVersion, nameof( actionDescriptor ) );

            var model = actionDescriptor.GetProperty<ApiVersionModel>();

            if ( model != null )
            {
                if ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion ) )
                {
                    return true;
                }

                if ( model.DeclaredApiVersions.Count > 0 )
                {
                    return false;
                }
            }

            model = actionDescriptor.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

            return model != null && ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion ) );
        }

        /// <summary>
        /// Occurs after the providers have been executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        /// <remarks>The default implementation performs no action.</remarks>
        public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
        {
            var results = context.Results;

            if ( results.Count == 0 )
            {
                return;
            }

            var groupResults = new List<ApiDescription>();

            foreach ( var version in FlattenApiVersions( results ) )
            {
                var groupName = GroupNameFormatter.GetGroupName( version );

                foreach ( var result in results )
                {
                    var action = result.ActionDescriptor;

                    if ( ShouldExploreAction( action, version ) )
                    {
                        // BUG: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/315
                        // BUG: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/355
                        // HACK: this happens when the the ApiVersionRouteConstraint is used. it doesn't produce model metadata; not even string. can it be prevented beyond the Swagger/Swashuckle fix?
                        foreach ( var param in result.ParameterDescriptions )
                        {
                            if ( param.ModelMetadata == null )
                            {
                                param.ModelMetadata = metadadataProvider.GetMetadataForType( typeof( string ) );
                            }
                        }

                        var groupResult = result.Clone();

                        groupResult.GroupName = groupName;
                        groupResult.SetProperty( version );
                        groupResults.Add( groupResult );
                    }
                }
            }

            results.Clear();

            foreach ( var result in groupResults )
            {
                results.Add( result );
            }
        }

        /// <summary>
        /// Occurs when the providers are being executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        public virtual void OnProvidersExecuting( ApiDescriptionProviderContext context ) { }

        static IEnumerable<ApiVersion> FlattenApiVersions( IEnumerable<ApiDescription> descriptions )
        {
            Contract.Requires( descriptions != null );
            Contract.Ensures( Contract.Result<IEnumerable<ApiVersion>>() != null );

            var versions = new HashSet<ApiVersion>();

            foreach ( var description in descriptions )
            {
                var action = description.ActionDescriptor;
                var model = action.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;
                var implicitModel = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

                foreach ( var version in model.DeclaredApiVersions.Union( implicitModel.DeclaredApiVersions ) )
                {
                    versions.Add( version );
                }
            }

            return versions.OrderBy( v => v );
        }
    }
}