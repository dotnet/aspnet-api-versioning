// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Mvc.ApiExplorer;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.ComponentModel;
using System.Globalization;
using static Asp.Versioning.ApiVersionMapping;
using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;

/// <summary>
/// Provides extension methods for the <see cref="ApiDescription"/> class.
/// </summary>
[CLSCompliant( false )]
public static class ApiDescriptionExtensions
{
    /// <summary>
    /// Gets the API version associated with the API description, if any.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the API version for.</param>
    /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
    public static ApiVersion? GetApiVersion( this ApiDescription apiDescription ) => apiDescription.GetProperty<ApiVersion>();

    /// <summary>
    /// Gets the API sunset policy associated with the API description, if any.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the sunset policy for.</param>
    /// <returns>The defined sunset policy defined for the API or <c>null</c>.</returns>
    public static SunsetPolicy? GetSunsetPolicy( this ApiDescription apiDescription ) => apiDescription.GetProperty<SunsetPolicy>();

    /// <summary>
    /// Gets a value indicating whether the associated API description is deprecated.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to evaluate.</param>
    /// <returns><c>True</c> if the <see cref="ApiDescription">API description</see> is deprecated; otherwise, <c>false</c>.</returns>
    public static bool IsDeprecated( this ApiDescription apiDescription )
    {
        ArgumentNullException.ThrowIfNull( apiDescription );

        var metatadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();

        if ( metatadata.IsApiVersionNeutral )
        {
            return false;
        }

        var apiVersion = apiDescription.GetApiVersion();
        var model = metatadata.Map( Explicit | Implicit );

        return model.DeprecatedApiVersions.Contains( apiVersion );
    }

    /// <summary>
    /// Sets the API version associated with the API description.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to set the API version for.</param>
    /// <param name="apiVersion">The associated <see cref="ApiVersion">API version</see>.</param>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetApiVersion( this ApiDescription apiDescription, ApiVersion apiVersion ) => apiDescription.SetProperty( apiVersion );

    /// <summary>
    /// Sets the API sunset policy associated with the API description.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to set the sunset policy for.</param>
    /// <param name="sunsetPolicy">The associated <see cref="SunsetPolicy">sunst policy</see>.</param>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetSunsetPolicy( this ApiDescription apiDescription, SunsetPolicy sunsetPolicy ) => apiDescription.SetProperty( sunsetPolicy );

    /// <summary>
    /// Attempts to update the relate path of the specified API description and remove the corresponding parameter according to the specified options.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to attempt to update.</param>
    /// <param name="options">The current <see cref="ApiExplorerOptions">API Explorer options</see>.</param>
    /// <returns>True if the <paramref name="apiDescription">API description</paramref> was updated; otherwise, false.</returns>
    public static bool TryUpdateRelativePathAndRemoveApiVersionParameter( this ApiDescription apiDescription, ApiExplorerOptions options )
    {
        ArgumentNullException.ThrowIfNull( apiDescription );
        ArgumentNullException.ThrowIfNull( options );

        if ( !options.SubstituteApiVersionInUrl )
        {
            return false;
        }

        var relativePath = apiDescription.RelativePath;

        if ( string.IsNullOrEmpty( relativePath ) )
        {
            return false;
        }

        if ( apiDescription.GetApiVersion() is not ApiVersion apiVersion )
        {
            return false;
        }

        var parameters = apiDescription.ParameterDescriptions;
        var parameter = parameters.FirstOrDefault( pd => pd.Source == Path && pd.ModelMetadata?.DataTypeName == nameof( ApiVersion ) );

        if ( parameter == null )
        {
            return false;
        }

        var token = '{' + parameter.Name + '}';
        var value = apiVersion.ToString( options.SubstitutionFormat, CultureInfo.InvariantCulture );
        var newRelativePath = relativePath.Replace( token, value, StringComparison.Ordinal );

        if ( relativePath == newRelativePath )
        {
            return false;
        }

        apiDescription.RelativePath = newRelativePath;
        parameters.Remove( parameter );
        return true;
    }

    /// <summary>
    /// Creates a shallow copy of the current API description.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to create a copy of.</param>
    /// <returns>A new <see cref="ApiDescription">API description</see>.</returns>
    public static ApiDescription Clone( this ApiDescription apiDescription )
    {
        ArgumentNullException.ThrowIfNull( apiDescription );

        var clone = new ApiDescription()
        {
            ActionDescriptor = apiDescription.ActionDescriptor,
            GroupName = apiDescription.GroupName,
            HttpMethod = apiDescription.HttpMethod,
            RelativePath = apiDescription.RelativePath,
        };

        foreach ( var property in apiDescription.Properties )
        {
            clone.Properties.Add( property );
        }

        CloneList( apiDescription.ParameterDescriptions, clone.ParameterDescriptions, Clone );
        CloneList( apiDescription.SupportedRequestFormats, clone.SupportedRequestFormats, Clone );
        CloneList( apiDescription.SupportedResponseTypes, clone.SupportedResponseTypes, Clone );

        return clone;
    }

    internal static ApiRequestFormat Clone( this ApiRequestFormat requestFormat )
    {
        return new()
        {
            Formatter = requestFormat.Formatter,
            MediaType = requestFormat.MediaType,
        };
    }

    internal static ApiResponseType Clone( this ApiResponseType responseType )
    {
        var clone = new ApiResponseType()
        {
            IsDefaultResponse = responseType.IsDefaultResponse,
            ModelMetadata = responseType.ModelMetadata,
            StatusCode = responseType.StatusCode,
            Type = responseType.Type,
        };

        CloneList( responseType.ApiResponseFormats, clone.ApiResponseFormats, Clone );

        return clone;
    }

    private static ApiParameterDescription Clone( ApiParameterDescription parameterDescription )
    {
        return new()
        {
            BindingInfo = parameterDescription.BindingInfo,
            IsRequired = parameterDescription.IsRequired,
            DefaultValue = parameterDescription.DefaultValue,
            ModelMetadata = parameterDescription.ModelMetadata,
            Name = parameterDescription.Name,
            ParameterDescriptor = parameterDescription.ParameterDescriptor,
            RouteInfo = parameterDescription.RouteInfo,
            Source = parameterDescription.Source,
            Type = parameterDescription.Type,
        };
    }

    private static ApiResponseFormat Clone( ApiResponseFormat responseFormat )
    {
        return new()
        {
            Formatter = responseFormat.Formatter,
            MediaType = responseFormat.MediaType,
        };
    }

    private static void CloneList<T>( IList<T> source, IList<T> destintation, Func<T, T> clone )
    {
        for ( var i = 0; i < source.Count; i++ )
        {
            destintation.Add( clone( source[i] ) );
        }
    }
}