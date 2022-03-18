// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
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
        ApiDescription = apiDescription;
        ApiVersion = apiVersion;
        ModelMetadata = modelMetadata;
        optional = options.AssumeDefaultVersionWhenUnspecified && apiVersion == options.DefaultApiVersion;
    }

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
        var query = from description in ApiDescription.ParameterDescriptions
                    let routeInfo = description.RouteInfo
                    where routeInfo != null
                    let constraints = routeInfo.Constraints ?? Empty<IRouteConstraint>()
                    where constraints.OfType<ApiVersionRouteConstraint>().Any()
                    select description;
        var parameter = query.FirstOrDefault();

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

        if ( parameter.ParameterDescriptor == null )
        {
            parameter.ParameterDescriptor = new ParameterDescriptor()
            {
                Name = parameter.Name,
                ParameterType = typeof( ApiVersion ),
            };
        }

        RemoveAllParametersExcept( parameter );
    }

    /// <summary>
    /// Adds the description for an API version expressed as a media type parameter.
    /// </summary>
    /// <param name="name">The name of the media type parameter.</param>
    protected virtual void AddMediaTypeParameter( string name )
    {
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
}