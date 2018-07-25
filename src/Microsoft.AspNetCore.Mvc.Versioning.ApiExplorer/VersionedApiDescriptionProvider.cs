﻿namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
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
            Arg.NotNull( metadataProvider, nameof( metadataProvider ) );
            Arg.NotNull( options, nameof( options ) );

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
        /// Gets the order prescendence of the current API description provider.
        /// </summary>
        /// <value>The order prescendence of the current API description provider. The default value is 0.</value>
        public virtual int Order => 0;

        /// <summary>
        /// Determines whether the specified action should be explored for the indicated API version.
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
        /// Populates the API version parameters for the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
        protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var action = apiDescription.ActionDescriptor;
            var model = action.GetProperty<ApiVersionModel>();

            if ( model.IsApiVersionNeutral )
            {
                return;
            }
            else if ( model.DeclaredApiVersions.Count == 0 )
            {
                model = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

                if ( model?.IsApiVersionNeutral == true )
                {
                    return;
                }
            }

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
            var results = context.Results;

            if ( results.Count == 0 )
            {
                return;
            }

            var groupResults = new List<ApiDescription>();
            var parameterSource = Options.ApiVersionParameterSource;

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

                    if ( Options.SubstituteApiVersionInUrl )
                    {
                        UpdateRelativePathAndRemoveApiVersionParameterIfNecessary( groupResult, Options.SubstitutionFormat );
                    }

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

        static void UpdateRelativePathAndRemoveApiVersionParameterIfNecessary( ApiDescription apiDescription, string apiVersionFormat )
        {
            Contract.Requires( apiDescription != null );

            var parameter = apiDescription.ParameterDescriptions.FirstOrDefault( pd => pd.Source == BindingSource.Path && pd.ModelMetadata?.DataTypeName == nameof( ApiVersion ) );

            if ( parameter == null )
            {
                return;
            }

            var relativePath = apiDescription.RelativePath;
            var token = '{' + parameter.Name + '}';
            var value = apiDescription.GetApiVersion().ToString( apiVersionFormat, InvariantCulture );
            var newRelativePath = relativePath.Replace( token, value );

            if ( relativePath != newRelativePath )
            {
                apiDescription.RelativePath = newRelativePath;
                apiDescription.ParameterDescriptions.Remove( parameter );
            }
        }

        IEnumerable<ApiVersion> FlattenApiVersions( IEnumerable<ApiDescription> descriptions )
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