// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.ModelBuilder.Config;
#endif
#if NETFRAMEWORK
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
#else
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
#endif

/// <summary>
/// Represents an OData query options convention based on <see cref="ODataValidationSettings">validation settings</see>.
/// </summary>
public partial class ODataValidationSettingsConvention : IODataQueryOptionsConvention
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataValidationSettingsConvention"/> class.
    /// </summary>
    /// <param name="validationSettings">The <see cref="ODataValidationSettings">validation settings</see> the convention is based on.</param>
    /// <param name="settings">The <see cref="ODataQueryOptionSettings">settings</see> used by the convention.</param>
    public ODataValidationSettingsConvention( ODataValidationSettings validationSettings, ODataQueryOptionSettings settings )
    {
        ValidationSettings = validationSettings;
        Settings = settings;
    }

    /// <summary>
    /// Gets the validation settings used for the query options convention.
    /// </summary>
    /// <value>The <see cref="ODataValidationSettings">validation settings</see> for the convention.</value>
    protected ODataValidationSettings ValidationSettings { get; }

    /// <summary>
    /// Gets the settings for OData query options.
    /// </summary>
    /// <value>The <see cref="ODataQueryOptionSettings">settings</see> used by the convention.</value>
    protected ODataQueryOptionSettings Settings { get; }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $filter query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewFilterParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Filter, descriptionContext );
        return NewParameterDescription( GetName( Filter ), description, typeof( string ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $expand query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewExpandParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Expand, descriptionContext );
        return NewParameterDescription( GetName( Expand ), description, typeof( string ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $select query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewSelectParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Select, descriptionContext );
        return NewParameterDescription( GetName( Select ), description, typeof( string ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $orderby query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewOrderByParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( OrderBy, descriptionContext );
        return NewParameterDescription( GetName( OrderBy ), description, typeof( string ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $top query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewTopParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Top, descriptionContext );
        return NewParameterDescription( GetName( Top ), description, typeof( int ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $skip query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewSkipParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Skip, descriptionContext );
        return NewParameterDescription( GetName( Skip ), description, typeof( int ) );
    }

    /// <summary>
    /// Creates and returns a new parameter descriptor for the $count query option.
    /// </summary>
    /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
    /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
    protected virtual ApiParameterDescription NewCountParameter( ODataQueryOptionDescriptionContext descriptionContext )
    {
        var description = Settings.DescriptionProvider.Describe( Count, descriptionContext );
        return NewParameterDescription( GetName( Count ), description, typeof( bool ), defaultValue: false );
    }

    // REF: http://docs.oasis-open.org/odata/odata/v4.01/cs01/part2-url-conventions/odata-v4.01-cs01-part2-url-conventions.html#sec_SystemQueryOptions
    private static bool IsSupported( string? httpMethod )
    {
        if ( string.IsNullOrEmpty( httpMethod ) )
        {
            return false;
        }

        return httpMethod!.ToUpperInvariant() switch
        {
            "GET" or "PUT" or "PATCH" or "POST" => true,
            _ => false,
        };
    }

    private string GetName( AllowedQueryOptions option )
    {
#pragma warning disable IDE0079
#pragma warning disable CA1308 // Normalize strings to uppercase (proper casing is lowercase)
        var name = option.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
#pragma warning restore IDE0079
        return Settings.NoDollarPrefix ? name : name.Insert( 0, "$" );
    }

    private AllowedQueryOptions GetQueryOptions( DefaultQuerySettings settings, ODataQueryOptionDescriptionContext context )
    {
        var queryOptions = ValidationSettings.AllowedQueryOptions;

        if ( settings.EnableCount )
        {
            queryOptions |= Count;
        }

        if ( settings.EnableExpand )
        {
            queryOptions |= Expand;
        }

        if ( settings.EnableFilter )
        {
            queryOptions |= Filter;
        }

        if ( settings.EnableOrderBy )
        {
            queryOptions |= OrderBy;
        }

        if ( settings.EnableSelect )
        {
            queryOptions |= Select;
        }

        if ( settings.MaxTop.NoLimitOrSome() )
        {
            context.MaxTop = settings.MaxTop;
        }

        return queryOptions;
    }
}