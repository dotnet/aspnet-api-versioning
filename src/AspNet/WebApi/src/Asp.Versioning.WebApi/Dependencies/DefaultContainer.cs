// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dependencies;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using System.ComponentModel.Design;
using System.Web.Http.Dependencies;

internal sealed class DefaultContainer : IDependencyResolver, IDependencyScope
{
    private readonly ServiceContainer container = new();
    private bool disposed;

    internal DefaultContainer()
    {
        container.AddService( typeof( ApiVersioningOptions ), static ( sc, t ) => new ApiVersioningOptions() );
        container.AddService( typeof( IApiVersionParser ), static ( sc, t ) => ApiVersionParser.Default );
        container.AddService( typeof( IControllerNameConvention ), static ( sc, t ) => ControllerNameConvention.Default );
        container.AddService( typeof( IProblemDetailsFactory ), static ( sc, t ) => new ProblemDetailsFactory() );
        container.AddService( typeof( IPolicyManager<SunsetPolicy> ), NewSunsetPolicyManager );
        container.AddService( typeof( IPolicyManager<DeprecationPolicy> ), NewDeprecationPolicyManager );
        container.AddService( typeof( IReportApiVersions ), NewApiVersionReporter );
    }

    public ApiVersioningOptions ApiVersioningOptions
    {
        get => GetApiVersioningOptions( container );
        set
        {
            container.RemoveService( typeof( ApiVersioningOptions ) );
            container.AddService( typeof( ApiVersioningOptions ), value );
        }
    }

    public void Replace( Type serviceType, ServiceCreatorCallback activator )
    {
        container.RemoveService( serviceType );
        container.AddService( serviceType, activator );
    }

    public IDependencyScope BeginScope() => this;

    public void Dispose()
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;
        container.Dispose();
    }

    public object GetService( Type serviceType ) => container.GetService( serviceType );

    public IEnumerable<object> GetServices( Type serviceType )
    {
        var service = container.GetService( serviceType );

        if ( service is not null )
        {
            yield return service;
        }
    }

    private static ApiVersioningOptions GetApiVersioningOptions( IServiceProvider serviceProvider ) =>
        (ApiVersioningOptions) serviceProvider.GetService( typeof( ApiVersioningOptions ) );

    private static IPolicyManager<SunsetPolicy> NewSunsetPolicyManager( IServiceProvider serviceProvider, Type type ) =>
        new SunsetPolicyManager( GetApiVersioningOptions( serviceProvider ) );

    private static IPolicyManager<DeprecationPolicy> NewDeprecationPolicyManager( IServiceProvider serviceProvider, Type type ) =>
        new DeprecationPolicyManager( GetApiVersioningOptions( serviceProvider ) );

    private static IReportApiVersions NewApiVersionReporter( IServiceProvider serviceProvider, Type type )
    {
        var options = GetApiVersioningOptions( serviceProvider );

        if ( options.ReportApiVersions )
        {
            var sunsetPolicyManager = (IPolicyManager<SunsetPolicy>) serviceProvider.GetService( typeof( IPolicyManager<SunsetPolicy> ) );
            var deprecationPolicyManager = (IPolicyManager<DeprecationPolicy>) serviceProvider.GetService( typeof( IPolicyManager<DeprecationPolicy> ) );
            return new DefaultApiVersionReporter( sunsetPolicyManager, deprecationPolicyManager );
        }

        return new DoNotReportApiVersions();
    }
}