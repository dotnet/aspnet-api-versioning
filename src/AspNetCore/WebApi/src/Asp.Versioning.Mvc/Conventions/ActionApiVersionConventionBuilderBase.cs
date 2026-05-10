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

        if ( VersionNeutral || ( apiModel = controller.ApiVersionModel ).IsApiVersionNeutral )
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
            var effectiveMapped = ExpandMappedVersions( apiModel.DeclaredApiVersions );
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
            else if ( !HasMappedVersions )
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
                var supportedVersions = HasIntroducedVersions ? inheritedSupported.Intersect( effectiveMapped ) : inheritedSupported;
                var deprecatedVersions = HasIntroducedVersions ? inheritedDeprecated.Intersect( effectiveMapped ) : inheritedDeprecated;

                endpointModel = new(
                    declaredVersions: effectiveMapped,
                    supportedVersions: supportedVersions,
                    deprecatedVersions: deprecatedVersions,
                    advertisedVersions: emptyVersions,
                    deprecatedAdvertisedVersions: emptyVersions );
            }

            metadata = new( apiModel, endpointModel, name, GetIntroducedApiVersionMetadata() );
        }

        item.AddEndpointMetadata( metadata );

        if ( !metadata.IsApiVersionNeutral )
        {
            var introducedItems = metadata.IntroducedInApiVersions;
            for ( var i = 0; i < introducedItems.Count; i++ )
            {
                item.AddEndpointMetadata( introducedItems[i] );
            }
        }
    }
}