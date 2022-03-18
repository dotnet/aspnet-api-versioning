// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

public class ApiVersioningMvcOptionsSetupTest
{
    [Fact]
    public void post_configure_should_not_register_report_filter_by_default()
    {
        // arrange
        var versioningOptions = Options.Create( new ApiVersioningOptions() );
        var mvcOptions = new MvcOptions();
        var setup = new ApiVersioningMvcOptionsSetup( versioningOptions );

        // act
        setup.PostConfigure( default, mvcOptions );

        // assert
        mvcOptions.Filters.Should().BeEmpty();
    }

    [Fact]
    public void post_configure_should_register_report_filter()
    {
        // arrange
        var versioningOptions = Options.Create( new ApiVersioningOptions() { ReportApiVersions = true } );
        var mvcOptions = new MvcOptions();
        var setup = new ApiVersioningMvcOptionsSetup( versioningOptions );

        // act
        setup.PostConfigure( default, mvcOptions );

        // assert
        mvcOptions.Filters.OfType<ServiceFilterAttribute>().Single().ServiceType.Should().Be( typeof( ReportApiVersionsAttribute ) );
    }

    [Fact]
    public void post_configure_should_register_model_binder_provider()
    {
        // arrange
        var versioningOptions = Options.Create( new ApiVersioningOptions() );
        var mvcOptions = new MvcOptions();
        var setup = new ApiVersioningMvcOptionsSetup( versioningOptions );
        var metadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( ApiVersion ) ) );
        var context = new Mock<ModelBinderProviderContext>();

        context.SetupGet( c => c.Metadata ).Returns( metadata.Object );

        // act
        setup.PostConfigure( default, mvcOptions );

        // assert
        mvcOptions.ModelBinderProviders.First().GetBinder( context.Object ).Should().BeOfType( typeof( ApiVersionModelBinder ) );
    }
}