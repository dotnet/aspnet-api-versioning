// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using static Asp.Versioning.ApiVersionParameterLocation;
using static System.Linq.Enumerable;
using static System.StringComparison;

/// <summary>
/// Represents an object that contains API version parameter descriptions.
/// </summary>
public class ApiVersionParameterDescriptionContext : IApiVersionParameterDescriptionContext
{
    private const int MaxApiVersionLocations = 4;
    private readonly List<ApiParameterDescription> parameters = new( capacity: MaxApiVersionLocations );
    private bool? versionNeutral;
    private bool optional;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionParameterDescriptionContext"/> class.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription"/> to provide API version parameter descriptions for.</param>
    /// <param name="apiVersion">The current API version.</param>
    /// <param name="modelMetadata">The <see cref="ModelMetadata">metadata</see> for the API version parameters.</param>
    /// <param name="options">The configured <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    [CLSCompliant( false )]
    public ApiVersionParameterDescriptionContext(
        ApiDescription apiDescription,
        ApiVersion apiVersion,
        ModelMetadata modelMetadata,
        ApiExplorerOptions options )
    {
        Options = options ?? throw new ArgumentNullException( nameof( options ) );
        ApiDescription = apiDescription ?? throw new ArgumentNullException( nameof( apiDescription ) );
        ApiVersion = apiVersion ?? throw new ArgumentNullException( nameof( apiVersion ) );
        ModelMetadata = modelMetadata ?? throw new ArgumentNullException( nameof( modelMetadata ) );
        optional = FirstParameterIsOptional( apiDescription, apiVersion, options );
    }

    // intentionally an internal property so the public contract doesn't change. this will be removed
    // once the ASP.NET Core team fixes the bug
    // BUG: https://github.com/dotnet/aspnetcore/issues/41773
    internal IInlineConstraintResolver? ConstraintResolver { get; set; }

    /// <summary>
    /// Gets the associated API description.
    /// </summary>
    /// <value>The associated <see cref="ApiDescription">API description</see>.</value>
    [CLSCompliant( false )]
    protected ApiDescription ApiDescription { get; }

    /// <summary>
    /// Gets the associated API version.
    /// </summary>
    /// <value>The associated <see cref="ApiVersion">API version</see>.</value>
    protected ApiVersion ApiVersion { get; }

    /// <summary>
    /// Gets a value indicating whether the current API is version-neutral.
    /// </summary>
    /// <value>True if the current API is version-neutral; otherwise, false.</value>
    protected bool IsApiVersionNeutral
    {
        get
        {
            if ( !versionNeutral.HasValue )
            {
                versionNeutral = ApiDescription.ActionDescriptor.GetApiVersionMetadata().IsApiVersionNeutral;
            }

            return versionNeutral.Value;
        }
    }

    /// <summary>
    /// Gets the model metadata for API version parameters.
    /// </summary>
    /// <value>The <see cref="ModelMetadata">model metadata</see> for the API version parameter.</value>
    [CLSCompliant( false )]
    protected ModelMetadata ModelMetadata { get; }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The configured <see cref="ApiExplorerOptions">API explorer options</see>.</value>
    protected ApiExplorerOptions Options { get; }

    private bool HasPathParameter
    {
        get
        {
            var query = from description in ApiDescription.ParameterDescriptions
                        where description.Source == BindingSource.Path &&
                              description.ModelMetadata?.DataTypeName == nameof( ApiVersion )
                        let constraints = description.RouteInfo?.Constraints ?? Empty<IRouteConstraint>()
                        where constraints.OfType<ApiVersionRouteConstraint>().Any()
                        select description;

            return query.Any();
        }
    }

    /// <summary>
    /// Adds an API version parameter with the specified name, from the specified location.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="location">One of the <see cref="ApiVersionParameterLocation"/> values.</param>
    public virtual void AddParameter( string name, ApiVersionParameterLocation location )
    {
        if ( IsApiVersionNeutral && !Options.AddApiVersionParametersWhenVersionNeutral )
        {
            return;
        }

        switch ( location )
        {
            case Query:
                AddQueryString( name );
                break;
            case Header:
                AddHeader( name );
                break;
            case Path:
                UpdateUrlSegment();
                break;
            case MediaTypeParameter:
                AddMediaTypeParameter( name );
                break;
        }
    }

    /// <summary>
    /// Adds the description for an API version expressed as a query string parameter.
    /// </summary>
    /// <param name="name">The name of the query string parameter.</param>
    protected virtual void AddQueryString( string name )
    {
        if ( !HasPathParameter )
        {
            ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, BindingSource.Query ) );
        }
    }

    /// <summary>
    /// Adds the description for an API version expressed as a header.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    protected virtual void AddHeader( string name )
    {
        if ( !HasPathParameter )
        {
            ApiDescription.ParameterDescriptions.Add( NewApiVersionParameter( name, BindingSource.Header ) );
        }
    }

    /// <summary>
    /// Adds the description for an API version expressed as a route parameter in a URL segment.
    /// </summary>
    protected virtual void UpdateUrlSegment()
    {
        var parameter = FindByRouteConstraintType( ApiDescription ) ??
                        FindByRouteConstraintName( ApiDescription, Options.RouteConstraintName ) ??
                        TryCreateFromRouteTemplate( ApiDescription, ConstraintResolver );

        if ( parameter == null )
        {
            return;
        }

        parameter.IsRequired = true;
        parameter.DefaultValue = ApiVersion.ToString();
        parameter.ModelMetadata = ModelMetadata;
        parameter.Type = ModelMetadata.ModelType;

        if ( parameter.RouteInfo != null )
        {
            parameter.RouteInfo.IsOptional = false;
            parameter.RouteInfo.DefaultValue = parameter.DefaultValue;
        }

        parameter.ParameterDescriptor ??= new()
        {
            Name = parameter.Name,
            ParameterType = typeof( ApiVersion ),
        };

        RemoveAllParametersExcept( parameter );
    }

    /// <summary>
    /// Adds the description for an API version expressed as a media type parameter.
    /// </summary>
    /// <param name="name">The name of the media type parameter.</param>
    protected virtual void AddMediaTypeParameter( string name )
    {
        if ( string.IsNullOrEmpty( name ) )
        {
            return;
        }

        var requestFormats = ApiDescription.SupportedRequestFormats.ToArray();
        var responseTypes = ApiDescription.SupportedResponseTypes.ToArray();
        var parameter = $"{name}={ApiVersion}";

        ApiDescription.SupportedRequestFormats.Clear();
        ApiDescription.SupportedResponseTypes.Clear();

        for ( var i = 0; i < requestFormats.Length; i++ )
        {
            var requestFormat = requestFormats[i];

            if ( requestFormat.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) < 0 )
            {
                requestFormat = requestFormat.Clone();
                requestFormat.MediaType += "; " + parameter;
            }

            ApiDescription.SupportedRequestFormats.Add( requestFormat );
        }

        for ( var i = 0; i < responseTypes.Length; i++ )
        {
            var responseType = responseTypes[i];
            var responseFormats = responseType.ApiResponseFormats;

            if ( !responseFormats.All( f => f.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) > 0 ) )
            {
                responseType = responseType.Clone();
                responseFormats = responseType.ApiResponseFormats;

                for ( var j = 0; j < responseFormats.Count; j++ )
                {
                    var responseFormat = responseFormats[j];

                    if ( responseFormat.MediaType.IndexOf( parameter, OrdinalIgnoreCase ) < 0 )
                    {
                        responseFormat.MediaType += "; " + parameter;
                    }
                }
            }

            ApiDescription.SupportedResponseTypes.Add( responseType );
        }
    }

    private static ApiParameterDescription? FindByRouteConstraintType( ApiDescription description )
    {
        var parameters = description.ParameterDescriptions;

        for ( var i = 0; i < parameters.Count; i++ )
        {
            var parameter = parameters[i];

            if ( parameter.RouteInfo is ApiParameterRouteInfo routeInfo &&
                 routeInfo.Constraints is IEnumerable<IRouteConstraint> constraints &&
                 constraints.OfType<ApiVersionRouteConstraint>().Any() )
            {
                return parameter;
            }
        }

        return default;
    }

    private static ApiParameterDescription? FindByRouteConstraintName( ApiDescription description, string constraintName )
    {
        var relativePath = description.RelativePath;

        if ( string.IsNullOrEmpty( relativePath ) )
        {
            return default;
        }

        var routePattern = RoutePatternFactory.Parse( relativePath );
        var parameters = routePattern.Parameters;
        var parameterDescriptions = description.ParameterDescriptions;

        for ( var i = 0; i < parameters.Count; i++ )
        {
            var parameter = parameters[i];
            var policies = parameter.ParameterPolicies;

            for ( var j = 0; j < policies.Count; j++ )
            {
                if ( !constraintName.Equals( policies[j].Content, Ordinal ) )
                {
                    continue;
                }

                for ( var k = 0; k < parameterDescriptions.Count; k++ )
                {
                    var parameterDescription = parameterDescriptions[k];

                    if ( parameterDescription.Name != parameter.Name &&
                         parameterDescription.ParameterDescriptor?.ParameterType != typeof( ApiVersion ) )
                    {
                        continue;
                    }

                    var token = $"{parameter.Name}:{constraintName}";

                    parameterDescription.Name = parameter.Name;
                    description.RelativePath = relativePath.Replace( token, parameter.Name, Ordinal );
                    parameterDescription.Source = BindingSource.Path;

                    return parameterDescription;
                }
            }
        }

        return default;
    }

    private static ApiParameterDescription? TryCreateFromRouteTemplate( ApiDescription description, IInlineConstraintResolver? constraintResolver )
    {
        if ( constraintResolver == null )
        {
            return default;
        }

        var relativePath = description.RelativePath;

        if ( string.IsNullOrEmpty( relativePath ) )
        {
            return default;
        }

        var constraints = new List<IRouteConstraint>();
        var template = TemplateParser.Parse( relativePath );
        var constraintName = default( string );

        for ( var i = 0; i < template.Parameters.Count; i++ )
        {
            var match = false;
            var parameter = template.Parameters[i];

            foreach ( var inlineConstraint in parameter.InlineConstraints )
            {
                var constraint = constraintResolver.ResolveConstraint( inlineConstraint.Constraint )!;

                constraints.Add( constraint );

                if ( constraint is ApiVersionRouteConstraint )
                {
                    match = true;
                    constraintName = inlineConstraint.Constraint;
                }
            }

            if ( !match )
            {
                continue;
            }

            constraints.TrimExcess();

            // ASP.NET Core does not discover route parameters without using Reflection in 6.0. unclear if it will be fixed before 7.0
            // BUG: https://github.com/dotnet/aspnetcore/issues/41773
            // REF: https://github.com/dotnet/aspnetcore/blob/release/6.0/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs#L323
            var result = new ApiParameterDescription()
            {
                Name = parameter.Name!,
                RouteInfo = new()
                {
                    Constraints = constraints,
                    DefaultValue = parameter.DefaultValue,
                    IsOptional = parameter.IsOptional || parameter.DefaultValue != null,
                },
                Source = BindingSource.Path,
            };
            var token = $"{parameter.Name}:{constraintName}";

            description.RelativePath = relativePath.Replace( token, parameter.Name, Ordinal );
            description.ParameterDescriptions.Insert( 0, result );
            return result;
        }

        return default;
    }

    private ApiParameterDescription NewApiVersionParameter( string name, BindingSource source )
    {
        var parameter = new ApiParameterDescription()
        {
            DefaultValue = ApiVersion.ToString(),
            IsRequired = !optional,
            ModelMetadata = ModelMetadata,
            Name = name,
            ParameterDescriptor = new()
            {
                Name = name,
                ParameterType = typeof( ApiVersion ),
            },
            Source = source,
            Type = ModelMetadata.ModelType,
        };

        if ( source == BindingSource.Path )
        {
            parameter.IsRequired = true;
            parameter.RouteInfo = new()
            {
                DefaultValue = ApiVersion.ToString(),
                IsOptional = false,
            };
        }

        optional = true;
        parameters.Add( parameter );

        return parameter;
    }

    private void RemoveAllParametersExcept( ApiParameterDescription parameter )
    {
        // in a scenario where multiple api version parameters are allowed, we can remove all other parameters because
        // the api version must be specified in the path. this will avoid unwanted, duplicate api version parameters
        var collections = new ICollection<ApiParameterDescription>[] { ApiDescription.ParameterDescriptions, parameters };

        for ( var i = 0; i < collections.Length; i++ )
        {
            var collection = collections[i];
            var otherParameters = collection.Where( p => p != parameter ).ToArray();

            for ( var j = 0; j < otherParameters.Length; j++ )
            {
                var otherParameter = otherParameters[j];

                if ( otherParameter.ModelMetadata?.DataTypeName == nameof( ApiVersion ) )
                {
                    collection.Remove( otherParameter );
                }
            }
        }
    }

    private static bool FirstParameterIsOptional(
        ApiDescription apiDescription,
        ApiVersion apiVersion,
        ApiExplorerOptions options )
    {
        if ( !options.AssumeDefaultVersionWhenUnspecified )
        {
            return false;
        }

        var mapping = ApiVersionMapping.Explicit | ApiVersionMapping.Implicit;
        var model = apiDescription.ActionDescriptor.GetApiVersionMetadata().Map( mapping );
        var defaultApiVersion = options.ApiVersionSelector.SelectVersion( model );

        return apiVersion == defaultApiVersion;
    }
}