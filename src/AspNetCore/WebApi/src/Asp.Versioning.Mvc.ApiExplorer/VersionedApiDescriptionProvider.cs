// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents an API explorer that provides <see cref="ApiDescription">API descriptions</see> for actions represented by
/// <see cref="ControllerActionDescriptor">controller action descriptors</see> that are <see cref="ApiVersion">API version</see> aware.
/// </summary>
[CLSCompliant( false )]
public class VersionedApiDescriptionProvider : IApiDescriptionProvider
{
    private readonly IOptions<ApiExplorerOptions> options;
    private readonly IInlineConstraintResolver constraintResolver;
    private ApiVersionModelMetadata? modelMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedApiDescriptionProvider"/> class.
    /// </summary>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</param>
    /// <param name="options">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public VersionedApiDescriptionProvider(
        ISunsetPolicyManager sunsetPolicyManager,
        IModelMetadataProvider modelMetadataProvider,
        IOptions<ApiExplorerOptions> options )
        : this(
              sunsetPolicyManager,
              modelMetadataProvider,
              new SimpleConstraintResolver( options ?? throw new ArgumentNullException( nameof( options ) ) ),
              options )
    {
    }

    // intentionally hiding IInlineConstraintResolver from public signature until ASP.NET Core fixes their bug
    // BUG: https://github.com/dotnet/aspnetcore/issues/41773
    internal VersionedApiDescriptionProvider(
        ISunsetPolicyManager sunsetPolicyManager,
        IModelMetadataProvider modelMetadataProvider,
        IInlineConstraintResolver constraintResolver,
        IOptions<ApiExplorerOptions> options )
    {
        SunsetPolicyManager = sunsetPolicyManager;
        ModelMetadataProvider = modelMetadataProvider;
        this.constraintResolver = constraintResolver;
        this.options = options;
    }

    /// <summary>
    /// Gets or sets the order precedence of the current API description provider.
    /// </summary>
    /// <value>The order precedence of the current API description provider. The default value is 0.</value>
    public int Order { get; protected set; }

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

    /// <summary>
    /// Gets the model metadata provider associated with the API description provider.
    /// </summary>
    /// <value>The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</value>
    protected IModelMetadataProvider ModelMetadataProvider { get; }

    private ApiVersionModelMetadata ModelMetadata =>
        modelMetadata ??= new( ModelMetadataProvider, Options.DefaultApiVersionParameterDescription );

    /// <summary>
    /// Determines whether the specified action should be explored for the indicated API version.
    /// </summary>
    /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> for action being explored.</param>
    /// <returns>True if the action should be explored; otherwise, false.</returns>
    protected virtual bool ShouldExploreAction( ActionDescriptor actionDescriptor, ApiVersion apiVersion ) =>
        actionDescriptor.GetApiVersionMetadata().IsMappedTo( apiVersion );

    /// <summary>
    /// Populates the API version parameters for the specified API description.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
    protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
    {
        var parameterSource = Options.ApiVersionParameterSource;
        var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, ModelMetadata, Options )
        {
            ConstraintResolver = constraintResolver,
        };

        parameterSource.AddParameters( context );
        apiDescription.TryUpdateRelativePathAndRemoveApiVersionParameter( Options );
    }

    /// <summary>
    /// Occurs after the providers have been executed.
    /// </summary>
    /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
    /// <remarks>The default implementation performs no action.</remarks>
    public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var results = context.Results;

        if ( results.Count == 0 )
        {
            return;
        }

        var groupResults = new List<ApiDescription>( capacity: results.Count );
        var unversioned = default( Dictionary<int, ApiDescription> );
        var formatGroupName = Options.FormatGroupName;
        var versions = FlattenApiVersions( results );

        for ( var i = 0; i < versions.Length; i++ )
        {
            var version = versions[i];
            var formattedVersion = version.ToString( Options.GroupNameFormat, CurrentCulture );

            for ( var j = 0; j < results.Count; j++ )
            {
                if ( unversioned != null && unversioned.ContainsKey( j ) )
                {
                    continue;
                }

                var result = results[j];
                var action = result.ActionDescriptor;

                if ( !ShouldExploreAction( action, version ) )
                {
                    if ( IsUnversioned( action ) )
                    {
                        unversioned ??= [];
                        unversioned.Add( j, result );
                    }

                    continue;
                }

                TryUpdateControllerRouteValueForMinimalApi( result );

                var groupResult = result.Clone();
                var metadata = action.GetApiVersionMetadata();

                if ( string.IsNullOrEmpty( groupResult.GroupName ) )
                {
                    groupResult.GroupName = formattedVersion;
                }
                else if ( formatGroupName is not null )
                {
                    groupResult.GroupName = formatGroupName( groupResult.GroupName, formattedVersion );
                }

                if ( SunsetPolicyManager.TryResolvePolicy( metadata.Name, version, out var policy ) )
                {
                    groupResult.SetSunsetPolicy( policy );
                }

                groupResult.SetApiVersion( version );
                PopulateApiVersionParameters( groupResult, version );
                AddOrUpdateResult( groupResults, groupResult, metadata, version );
            }
        }

        results.Clear();

        for ( var j = 0; j < groupResults.Count; j++ )
        {
            results.Add( groupResults[j] );
        }

        if ( unversioned == null )
        {
            return;
        }

        foreach ( var result in unversioned.Values )
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

    private static bool IsUnversioned( ActionDescriptor action )
    {
        var endpointMetadata = action.EndpointMetadata;

        if ( endpointMetadata == null )
        {
            return true;
        }

        for ( var i = 0; i < endpointMetadata.Count; i++ )
        {
            if ( endpointMetadata[i] is ApiVersionMetadata )
            {
                return false;
            }
        }

        return true;
    }

    private static void TryUpdateControllerRouteValueForMinimalApi( ApiDescription description )
    {
        var action = description.ActionDescriptor;

        if ( action is ControllerActionDescriptor )
        {
            return;
        }

        var routeValues = action.RouteValues;

        if ( !routeValues.ContainsKey( "controller" ) )
        {
            return;
        }

        var metadata = action.GetApiVersionMetadata();

        if ( !string.IsNullOrEmpty( metadata.Name ) )
        {
            routeValues["controller"] = metadata.Name;
        }
    }

    private static void AddOrUpdateResult(
        List<ApiDescription> results,
        ApiDescription result,
        ApiVersionMetadata metadata,
        ApiVersion version )
    {
        var comparer = StringComparer.OrdinalIgnoreCase;

        for ( var i = results.Count - 1; i >= 0; i-- )
        {
            var other = results[i];

            if ( comparer.Equals( result.GroupName, other.GroupName ) &&
                 comparer.Equals( result.RelativePath, other.RelativePath ) &&
                 comparer.Equals( result.HttpMethod, other.HttpMethod ) )
            {
                var mapping = other.ActionDescriptor.GetApiVersionMetadata().MappingTo( version );

                switch ( metadata.MappingTo( version ) )
                {
                    case Explicit:
                        if ( mapping == Implicit )
                        {
                            results.RemoveAt( i );
                        }

                        break;
                    case Implicit:
                        if ( mapping == Explicit )
                        {
                            return;
                        }

                        break;
                }
            }
        }

        results.Add( result );
    }

    private ApiVersion[] FlattenApiVersions( IList<ApiDescription> descriptions )
    {
        var versions = default( SortedSet<ApiVersion> );

        for ( var i = 0; i < descriptions.Count; i++ )
        {
            var action = descriptions[i].ActionDescriptor;
            var model = action.GetApiVersionMetadata().Map( Explicit | Implicit );
            var declaredVersions = model.DeclaredApiVersions;

            if ( versions is null && declaredVersions.Count > 0 )
            {
                versions = [];
            }

            for ( var j = 0; j < declaredVersions.Count; j++ )
            {
                versions!.Add( declaredVersions[j] );
            }
        }

        if ( versions is null )
        {
            return [Options.DefaultApiVersion];
        }

        return [.. versions];
    }

    private sealed class SimpleConstraintResolver( IOptions<ApiExplorerOptions> options ) : IInlineConstraintResolver
    {
        public IRouteConstraint? ResolveConstraint( string inlineConstraint )
        {
            if ( options.Value.RouteConstraintName == inlineConstraint )
            {
                return new ApiVersionRouteConstraint();
            }

            return default;
        }
    }
}