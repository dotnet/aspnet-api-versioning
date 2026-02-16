// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using System.Globalization;
using System.Web.Http;
using System.Web.Http.Dependencies;

internal static class DependencyResolverExtensions
{
    extension( IDependencyResolver resolver )
    {
        internal TService? GetService<TService>() => (TService) resolver.GetService( typeof( TService ) );

        internal TService GetRequiredService<TService>()
        {
            var service = resolver.GetService<TService>();
            var message = string.Format( CultureInfo.CurrentCulture, BackportSR.NoServiceRegistered, typeof( TService ) );
            return service ?? throw new InvalidOperationException( message );
        }
    }

    extension( HttpConfiguration configuration )
    {
        internal IApiVersionParser ApiVersionParser =>
            configuration.DependencyResolver.GetService<IApiVersionParser>() ??
            configuration.ApiVersioningServices.GetRequiredService<IApiVersionParser>();

        internal IReportApiVersions ApiVersionReporter =>
            configuration.DependencyResolver.GetService<IReportApiVersions>() ??
            configuration.ApiVersioningServices.GetRequiredService<IReportApiVersions>();

        internal IControllerNameConvention ControllerNameConvention =>
            configuration.DependencyResolver.GetService<IControllerNameConvention>() ??
            configuration.ApiVersioningServices.GetRequiredService<IControllerNameConvention>();

        internal IProblemDetailsFactory ProblemDetailsFactory =>
            configuration.DependencyResolver.GetService<IProblemDetailsFactory>() ??
            configuration.ApiVersioningServices.GetRequiredService<IProblemDetailsFactory>();

        internal IPolicyManager<SunsetPolicy> SunsetPolicyManager =>
            configuration.DependencyResolver.GetService<IPolicyManager<SunsetPolicy>>() ??
            configuration.ApiVersioningServices.GetRequiredService<IPolicyManager<SunsetPolicy>>();

        internal IPolicyManager<DeprecationPolicy> DeprecationPolicyManager =>
            configuration.DependencyResolver.GetService<IPolicyManager<DeprecationPolicy>>() ??
            configuration.ApiVersioningServices.GetRequiredService<IPolicyManager<DeprecationPolicy>>();
    }
}