// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ODataValidationSettingsConvention
{
    /// <inheritdoc />
    public virtual void ApplyTo( ApiDescription apiDescription )
    {
        if ( apiDescription == null )
        {
            throw new ArgumentNullException( nameof( apiDescription ) );
        }

        if ( !IsSupported( apiDescription.HttpMethod ) )
        {
            return;
        }

        var context = new ODataQueryOptionDescriptionContext( ValidationSettings );
        var model = ResolveModel( apiDescription );
        var queryOptions = GetQueryOptions( Settings.DefaultQuerySettings!, context );
        var singleResult = IsSingleResult( apiDescription, out var resultType );
        var visitor = new ODataAttributeVisitor( context, model, queryOptions, resultType, singleResult );

        visitor.Visit( apiDescription );

        var options = visitor.AllowedQueryOptions;
        var parameterDescriptions = apiDescription.ParameterDescriptions;

        if ( options.HasFlag( Select ) )
        {
            parameterDescriptions.Add( NewSelectParameter( context ) );
        }

        if ( options.HasFlag( Expand ) )
        {
            parameterDescriptions.Add( NewExpandParameter( context ) );
        }

        if ( singleResult )
        {
            return;
        }

        if ( options.HasFlag( Filter ) )
        {
            parameterDescriptions.Add( NewFilterParameter( context ) );
        }

        if ( options.HasFlag( OrderBy ) )
        {
            parameterDescriptions.Add( NewOrderByParameter( context ) );
        }

        if ( options.HasFlag( Top ) )
        {
            parameterDescriptions.Add( NewTopParameter( context ) );
        }

        if ( options.HasFlag( Skip ) )
        {
            parameterDescriptions.Add( NewSkipParameter( context ) );
        }

        if ( options.HasFlag( Count ) )
        {
            parameterDescriptions.Add( NewCountParameter( context ) );
        }
    }

    /// <summary>
    /// Creates a new API parameter description.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="description">The parameter description.</param>
    /// <param name="type">The parameter value type.</param>
    /// <param name="defaultValue">The parameter default value, if any.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewParameterDescription(
        string name,
        string description,
        Type type,
        object? defaultValue = default )
    {
        return new()
        {
            DefaultValue = defaultValue,
            IsRequired = false,
            ModelMetadata = new ODataQueryOptionModelMetadata(
                Settings.ModelMetadataProvider!,
                type,
                description ),
            Name = name,
            ParameterDescriptor = new()
            {
                Name = name,
                ParameterType = type,
            },
            Source = Query,
            Type = type,
        };
    }

    private static IEdmModel? ResolveModel( ApiDescription description )
    {
        var version = description.GetApiVersion();

        if ( version == null )
        {
            return default;
        }

        var items = description.ActionDescriptor.EndpointMetadata.OfType<IODataRoutingMetadata>();

        foreach ( var item in items )
        {
            var model = item.Model;
            var otherVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

            if ( version.Equals( otherVersion ) )
            {
                return model;
            }
        }

        return default;
    }

    private static bool IsSingleResult( ApiDescription description, out Type? resultType )
    {
        if ( description.SupportedResponseTypes.Count == 0 )
        {
            resultType = default;
            return true;
        }

        var supportedResponseTypes = description.SupportedResponseTypes;
        var candidates = default( List<ApiResponseType> );

        for ( var i = 0; i < supportedResponseTypes.Count; i++ )
        {
            var supportedResponseType = supportedResponseTypes[i];

            if ( supportedResponseType.Type == null )
            {
                continue;
            }

            var statusCode = supportedResponseType.StatusCode;

            if ( statusCode >= Status200OK && statusCode < Status300MultipleChoices )
            {
                candidates ??= new( supportedResponseTypes.Count );
                candidates.Add( supportedResponseType );
            }
        }

        if ( candidates == null || candidates.Count == 0 )
        {
            resultType = default;
            return true;
        }

        candidates.Sort( ( r1, r2 ) => r1.StatusCode.CompareTo( r2.StatusCode ) );

        if ( candidates[0].Type is not Type type )
        {
            resultType = default;
            return false;
        }

        var responseType = type.ExtractInnerType();

        if ( responseType.IsEnumerable( out resultType ) )
        {
            return false;
        }

        resultType = responseType;
        return true;
    }
}