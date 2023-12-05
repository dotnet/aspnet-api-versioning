// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ActionApiVersionConventionBuilderBase : IApiVersionConvention<ActionModel>
{
    /// <summary>
    /// Applies the builder conventions to the specified controller action.
    /// </summary>
    /// <param name="item">The <see cref="ActionModel">action model</see> to apply the conventions to.</param>
    public virtual void ApplyTo( ActionModel item )
    {
        ArgumentNullException.ThrowIfNull( item );

        MergeAttributesWithConventions( item.Attributes );

        var controller = item.Controller;
        var name = NamingConvention.GroupName( controller.ControllerName );
        ApiVersionModel apiModel;
        ApiVersionMetadata metadata;

        if ( VersionNeutral || ( apiModel = controller.GetApiVersionModel() ).IsApiVersionNeutral )
        {
            metadata = string.IsNullOrEmpty( name )
                       ? ApiVersionMetadata.Neutral
                       : new( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );
        }
        else
        {
            ApiVersionModel endpointModel;
            ApiVersion[] emptyVersions;
            var inheritedSupported = apiModel.SupportedApiVersions;
            var inheritedDeprecated = apiModel.DeprecatedApiVersions;
            var noInheritedApiVersions = inheritedSupported.Count == 0 &&
                                         inheritedDeprecated.Count == 0;

            if ( IsEmpty )
            {
                if ( noInheritedApiVersions )
                {
                    endpointModel = ApiVersionModel.Empty;
                }
                else
                {
                    emptyVersions = [];
                    endpointModel = new(
                        declaredVersions: emptyVersions,
                        inheritedSupported,
                        inheritedDeprecated,
                        emptyVersions,
                        emptyVersions );
                }
            }
            else if ( mapped is null || mapped.Count == 0 )
            {
                endpointModel = new(
                    declaredVersions: SupportedVersions.Union( DeprecatedVersions ),
                    SupportedVersions.Union( inheritedSupported ),
                    DeprecatedVersions.Union( inheritedDeprecated ),
                    AdvertisedVersions,
                    DeprecatedAdvertisedVersions );
            }
            else
            {
                emptyVersions = [];
                endpointModel = new(
                    declaredVersions: mapped,
                    supportedVersions: inheritedSupported,
                    deprecatedVersions: inheritedDeprecated,
                    advertisedVersions: emptyVersions,
                    deprecatedAdvertisedVersions: emptyVersions );
            }

            metadata = new( apiModel, endpointModel, name );
        }

        item.AddEndpointMetadata( metadata );
    }
}