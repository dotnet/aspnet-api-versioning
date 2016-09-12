namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="IHttpController">controller</see>.</typeparam>
    public partial class ActionApiVersionConventionBuilder<T> : IApiVersionConvention<HttpActionDescriptor>, IActionConventionBuilder<T> where T : IHttpController
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action descriptor</see>
        /// to apply the conventions to.</param>
        public void ApplyTo( HttpActionDescriptor actionDescriptor )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );

            mappedVersions.UnionWith( from provider in actionDescriptor.GetCustomAttributes<IApiVersionProvider>()
                                      where !provider.AdvertiseOnly && !provider.Deprecated
                                      from version in provider.Versions
                                      select version );

            var noVersions = Enumerable.Empty<ApiVersion>();
            var model = new ApiVersionModel(
                apiVersionNeutral: false,
                supported: mappedVersions,
                deprecated: noVersions,
                advertised: noVersions,
                deprecatedAdvertised: noVersions );

            actionDescriptor.SetApiVersionModel( model );
        }
    }
}