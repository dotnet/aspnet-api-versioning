// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

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
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the API version for.</param>
    extension( ApiDescription apiDescription )
    {
        /// <summary>
        /// Gets or sets the API version associated with the API description, if any.
        /// </summary>
        /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        /// <remarks>Setting this property is meant for infrastructure and should not be used by application code.</remarks>
        public ApiVersion? ApiVersion
        {
            get => apiDescription.GetProperty<ApiVersion>();

            [EditorBrowsable( EditorBrowsableState.Never )]
            set => apiDescription.SetProperty( value );
        }

        /// <summary>
        /// Gets or sets the API sunset policy associated with the API description, if any.
        /// </summary>
        /// <returns>The defined sunset policy defined for the API or <c>null</c>.</returns>
        /// <remarks>Setting this property is meant for infrastructure and should not be used by application code.</remarks>
        public SunsetPolicy? SunsetPolicy
        {
            get => apiDescription.GetProperty<SunsetPolicy>();

            [EditorBrowsable( EditorBrowsableState.Never )]
            set => apiDescription.SetProperty( value );
        }

        /// <summary>
        /// Gets or sets the API deprecation policy associated with the API description, if any.
        /// </summary>
        /// <returns>The defined deprecation policy defined for the API or <c>null</c>.</returns>
        /// <remarks>Setting this property is meant for infrastructure and should not be used by application code.</remarks>
        public DeprecationPolicy? DeprecationPolicy
        {
            get => apiDescription.GetProperty<DeprecationPolicy>();

            [EditorBrowsable( EditorBrowsableState.Never )]
            set => apiDescription.SetProperty( value );
        }

        /// <summary>
        /// Gets a value indicating whether the associated API description is deprecated.
        /// </summary>
        /// <returns><c>True</c> if the <see cref="ApiDescription">API description</see> is deprecated; otherwise, <c>false</c>.</returns>
        public bool IsDeprecated
        {
            get
            {
                ArgumentNullException.ThrowIfNull( apiDescription );

                var metatadata = apiDescription.ActionDescriptor.ApiVersionMetadata;

                if ( metatadata.IsApiVersionNeutral )
                {
                    if ( apiDescription.DeprecationPolicy is { } policy )
                    {
                        return policy.IsEffective( DateTimeOffset.Now );
                    }

                    return false;
                }

                var apiVersion = apiDescription.ApiVersion;
                var model = metatadata.Map( Explicit | Implicit );

                return model.DeprecatedApiVersions.Contains( apiVersion );
            }
        }

        /// <summary>
        /// Attempts to update the relate path of the specified API description and remove the corresponding parameter according to the specified options.
        /// </summary>
        /// <param name="options">The current <see cref="ApiExplorerOptions">API Explorer options</see>.</param>
        /// <returns>True if the API description was updated; otherwise, false.</returns>
        public bool TryUpdateRelativePathAndRemoveApiVersionParameter( ApiExplorerOptions options )
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

            if ( apiDescription.ApiVersion is not { } apiVersion )
            {
                return false;
            }

            var parameters = apiDescription.ParameterDescriptions;
            var parameter = parameters.FirstOrDefault( pd => pd.Source == Path && pd.ModelMetadata?.DataTypeName == nameof( ApiVersion ) );

            if ( parameter == null )
            {
                return false;
            }

            Span<char> token = stackalloc char[parameter.Name.Length + 2];

            token[0] = '{';
            token[^1] = '}';
            parameter.Name.AsSpan().CopyTo( token.Slice( 1, parameter.Name.Length ) );

            var value = apiVersion.ToString( options.SubstitutionFormat, CultureInfo.InvariantCulture );
            var newRelativePath = relativePath.Replace( token.ToString(), value, StringComparison.Ordinal );

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
        /// <returns>A new <see cref="ApiDescription">API description</see>.</returns>
        public ApiDescription Clone()
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
    }

    extension( ApiRequestFormat requestFormat )
    {
        internal ApiRequestFormat Clone() => new()
        {
            Formatter = requestFormat.Formatter,
            MediaType = requestFormat.MediaType,
        };
    }

    extension( ApiResponseType responseType )
    {
        internal ApiResponseType Clone()
        {
            var clone = new ApiResponseType()
            {
                Description = responseType.Description,
                IsDefaultResponse = responseType.IsDefaultResponse,
                ModelMetadata = responseType.ModelMetadata,
                StatusCode = responseType.StatusCode,
                Type = responseType.Type,
            };

            CloneList( responseType.ApiResponseFormats, clone.ApiResponseFormats, Clone );

            return clone;
        }
    }

    private static ApiParameterDescription Clone( ApiParameterDescription parameterDescription ) => new()
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

    private static ApiResponseFormat Clone( ApiResponseFormat responseFormat ) => new()
    {
        Formatter = responseFormat.Formatter,
        MediaType = responseFormat.MediaType,
    };

    private static void CloneList<T>( IList<T> source, IList<T> destintation, Func<T, T> clone )
    {
        for ( var i = 0; i < source.Count; i++ )
        {
            destintation.Add( clone( source[i] ) );
        }
    }
}