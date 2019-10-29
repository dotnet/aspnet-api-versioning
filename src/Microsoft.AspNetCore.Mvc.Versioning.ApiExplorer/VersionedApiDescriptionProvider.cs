namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.Globalization.CultureInfo;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents an API explorer that provides <see cref="ApiDescription">API descriptions</see> for actions represented by
    /// <see cref="ControllerActionDescriptor">controller action descriptors</see> that are <see cref="ApiVersion">API version</see> aware.
    /// </summary>
    [CLSCompliant( false )]
    public class VersionedApiDescriptionProvider : IApiDescriptionProvider
    {
        readonly IOptions<ApiExplorerOptions> options;
        readonly Lazy<ModelMetadata> modelMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedApiDescriptionProvider"/> class.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</param>
        /// <param name="options">The <see cref="IOptions{TOptions}">container</see> of configured <see cref="ApiExplorerOptions">API explorer options</see>.</param>
        public VersionedApiDescriptionProvider( IModelMetadataProvider metadataProvider, IOptions<ApiExplorerOptions> options )
        {
            MetadataProvider = metadataProvider;
            this.options = options;
            modelMetadata = new Lazy<ModelMetadata>( NewModelMetadata );
        }

        /// <summary>
        /// Gets the model metadata provider associated with the API description provider.
        /// </summary>
        /// <value>The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</value>
        protected IModelMetadataProvider MetadataProvider { get; }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ApiExplorerOptions Options => options.Value;

        /// <summary>
        /// Gets the order precedence of the current API description provider.
        /// </summary>
        /// <value>The order precedence of the current API description provider. The default value is 0.</value>
        public virtual int Order => 0;

        /// <summary>
        /// Determines whether the specified action should be explored for the indicated API version.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> for action being explored.</param>
        /// <returns>True if the action should be explored; otherwise, false.</returns>
        protected virtual bool ShouldExploreAction( ActionDescriptor actionDescriptor, ApiVersion apiVersion ) => actionDescriptor.IsMappedTo( apiVersion );

        /// <summary>
        /// Populates the API version parameters for the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
        protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
        {
            var parameterSource = Options.ApiVersionParameterSource;
            var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, modelMetadata.Value, Options );

            parameterSource.AddParameters( context );
        }

        /// <summary>
        /// Occurs after the providers have been executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        /// <remarks>The default implementation performs no action.</remarks>
        public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var results = context.Results;

            if ( results.Count == 0 )
            {
                return;
            }

            var groupResults = new List<ApiDescription>();

            foreach ( var version in FlattenApiVersions( results ) )
            {
                var groupName = version.ToString( Options.GroupNameFormat, CurrentCulture );

                foreach ( var result in results )
                {
                    var action = result.ActionDescriptor;

                    if ( !ShouldExploreAction( action, version ) )
                    {
                        continue;
                    }

                    var groupResult = result.Clone();

                    groupResult.GroupName = groupName;
                    groupResult.SetApiVersion( version );
                    PopulateApiVersionParameters( groupResult, version );
                    groupResult.TryUpdateRelativePathAndRemoveApiVersionParameter( Options );
                    groupResults.Add( groupResult );
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
        /// <remarks>The default implementation performs no operation.</remarks>
        public virtual void OnProvidersExecuting( ApiDescriptionProviderContext context ) { }

        IEnumerable<ApiVersion> FlattenApiVersions( IEnumerable<ApiDescription> descriptions )
        {
            var versions = new HashSet<ApiVersion>();

            foreach ( var description in descriptions )
            {
                var action = description.ActionDescriptor;
                var model = action.GetApiVersionModel( Explicit | Implicit );

                foreach ( var version in model.DeclaredApiVersions )
                {
                    versions.Add( version );
                }
            }

            if ( versions.Count == 0 )
            {
                versions.Add( Options.DefaultApiVersion );
                return versions;
            }

            return versions.OrderBy( v => v );
        }

        ModelMetadata NewModelMetadata() => new ApiVersionModelMetadata( MetadataProvider, Options.DefaultApiVersionParameterDescription );
    }
}