// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
        ArgumentNullException.ThrowIfNull( apiDescription );

        if ( !IsSupported( apiDescription.HttpMethod ) )
        {
            return;
        }

        var context = new ODataQueryOptionDescriptionContext( apiDescription, ValidationSettings );
        var queryOptions = GetQueryOptions( Settings.DefaultQuerySettings!, context );
        var visitor = new ODataAttributeVisitor( context, queryOptions );

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

        if ( context.IsSingleResult )
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
}