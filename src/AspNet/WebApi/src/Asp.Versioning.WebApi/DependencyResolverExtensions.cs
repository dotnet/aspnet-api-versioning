// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using System.Globalization;
using System.Web.Http;
using System.Web.Http.Dependencies;

internal static class DependencyResolverExtensions
{
    internal static TService? GetService<TService>( this IDependencyResolver resolver ) =>
        (TService) resolver.GetService( typeof( TService ) );

    internal static TService GetRequiredService<TService>( this IDependencyResolver resolver )
    {
        var service = resolver.GetService<TService>();
        var message = string.Format( CultureInfo.CurrentCulture, SR.NoServiceRegistered, typeof( TService ) );
        return service ?? throw new InvalidOperationException( message );
    }

    internal static IApiVersionParser GetApiVersionParser( this HttpConfiguration configuration ) =>
        configuration.DependencyResolver.GetService<IApiVersionParser>() ??
        configuration.ApiVersioningServices().GetRequiredService<IApiVersionParser>();

    internal static IReportApiVersions GetApiVersionReporter( this HttpConfiguration configuration ) =>
        configuration.DependencyResolver.GetService<IReportApiVersions>() ??
        configuration.ApiVersioningServices().GetRequiredService<IReportApiVersions>();

    internal static IControllerNameConvention GetControllerNameConvention( this HttpConfiguration configuration ) =>
        configuration.DependencyResolver.GetService<IControllerNameConvention>() ??
        configuration.ApiVersioningServices().GetRequiredService<IControllerNameConvention>();

    internal static IProblemDetailsFactory GetProblemDetailsFactory( this HttpConfiguration configuration ) =>
        configuration.DependencyResolver.GetService<IProblemDetailsFactory>() ??
        configuration.ApiVersioningServices().GetRequiredService<IProblemDetailsFactory>();

    internal static ISunsetPolicyManager GetSunsetPolicyManager( this HttpConfiguration configuration ) =>
        configuration.DependencyResolver.GetService<ISunsetPolicyManager>() ??
        configuration.ApiVersioningServices().GetRequiredService<ISunsetPolicyManager>();
}