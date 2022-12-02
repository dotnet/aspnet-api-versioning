// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using System.Web.Http.Dependencies;

internal static class DependencyResolverExtensions
{
    internal static TService? GetService<TService>( this IDependencyResolver resolver ) => (TService) resolver.GetService( typeof( TService ) );

    internal static IApiVersionParser GetApiVersionParser( this IDependencyResolver resolver ) =>
        resolver.GetService<IApiVersionParser>() ?? ApiVersionParser.Default;

    internal static IReportApiVersions GetApiVersionReporter( this IDependencyResolver resolver ) =>
        resolver.GetService<IReportApiVersions>() ?? DefaultApiVersionReporter.GetOrCreate( resolver.GetSunsetPolicyManager() );

    internal static IControllerNameConvention GetControllerNameConvention( this IDependencyResolver resolver ) =>
        resolver.GetService<IControllerNameConvention>() ?? ControllerNameConvention.Default;

    internal static IProblemDetailsFactory GetProblemDetailsFactory( this IDependencyResolver resolver ) =>
        resolver.GetService<IProblemDetailsFactory>() ?? ProblemDetailsFactory.Default;

    internal static ISunsetPolicyManager GetSunsetPolicyManager( this IDependencyResolver resolver ) =>
        resolver.GetService<ISunsetPolicyManager>() ?? SunsetPolicyManager.Default;
}