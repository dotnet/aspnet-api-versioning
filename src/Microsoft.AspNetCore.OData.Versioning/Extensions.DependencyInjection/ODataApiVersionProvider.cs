namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc.Versioning;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    sealed class ODataApiVersionProvider : IODataApiVersionProvider
    {
        public ODataApiVersionProvider( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            Contract.Requires( actionDescriptorCollectionProvider != null );

            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            foreach ( var model in ODataApiVersions( actionDescriptorCollectionProvider ) )
            {
                foreach ( var apiVersion in model.SupportedApiVersions )
                {
                    supported.Add( apiVersion );
                }

                foreach ( var apiVersion in model.DeprecatedApiVersions )
                {
                    deprecated.Add( apiVersion );
                }
            }

            deprecated.ExceptWith( supported );
            SupportedApiVersions = supported;
            DeprecatedApiVersions = deprecated;
        }

        public IReadOnlyCollection<ApiVersion> SupportedApiVersions { get; }

        public IReadOnlyCollection<ApiVersion> DeprecatedApiVersions { get; }

        static IEnumerable<ApiVersionModel> ODataApiVersions( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            Contract.Requires( actionDescriptorCollectionProvider != null );
            Contract.Ensures( Contract.Result<IEnumerable<ApiVersionModel>>() != null );

            var odataController = typeof( ODataController ).GetTypeInfo();

            foreach ( var action in actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>() )
            {
                var controller = action.ControllerTypeInfo;

                if ( odataController.IsAssignableFrom( action.ControllerTypeInfo ) )
                {
                    var model = action.GetProperty<ApiVersionModel>();

                    if ( model != null )
                    {
                        yield return model;
                    }
                }
            }
        }
    }
}