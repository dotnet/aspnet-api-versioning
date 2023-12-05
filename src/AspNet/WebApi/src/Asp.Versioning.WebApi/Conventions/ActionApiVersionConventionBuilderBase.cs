// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Web.Http;
using System.Web.Http.Controllers;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Web API.
/// </content>
public partial class ActionApiVersionConventionBuilderBase : IApiVersionConvention<HttpActionDescriptor>
{
    /// <inheritdoc />
    public virtual void ApplyTo( HttpActionDescriptor item )
    {
        ArgumentNullException.ThrowIfNull( item );

        var attributes = new List<object>();

        attributes.AddRange( item.GetCustomAttributes<IApiVersionNeutral>( inherit: true ) );
        attributes.AddRange( item.GetCustomAttributes<IApiVersionProvider>( inherit: false ) );
        MergeAttributesWithConventions( attributes );

        ApiVersionModel apiModel;
        ApiVersionMetadata metadata;
        var name = NamingConvention.GroupName( item.ControllerDescriptor.ControllerName );

        if ( VersionNeutral || ( apiModel = item.ControllerDescriptor.GetApiVersionModel() ).IsApiVersionNeutral )
        {
            metadata = string.IsNullOrEmpty( name )
                ? ApiVersionMetadata.Neutral
                : new ApiVersionMetadata( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );
        }
        else
        {
            ApiVersionModel endpointModel;
            IEnumerable<ApiVersion> emptyVersions;
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
                    emptyVersions = Enumerable.Empty<ApiVersion>();
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
                    SupportedVersions.Union( apiModel.SupportedApiVersions ),
                    DeprecatedVersions.Union( apiModel.DeprecatedApiVersions ),
                    AdvertisedVersions,
                    DeprecatedAdvertisedVersions );
            }
            else
            {
                emptyVersions = Enumerable.Empty<ApiVersion>();
                endpointModel = new(
                    declaredVersions: mapped,
                    supportedVersions: apiModel.SupportedApiVersions,
                    deprecatedVersions: apiModel.DeprecatedApiVersions,
                    advertisedVersions: emptyVersions,
                    deprecatedAdvertisedVersions: emptyVersions );
            }

            metadata = new( apiModel, endpointModel, name );
        }

        item.SetApiVersionMetadata( metadata );
    }
}