// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingMediaType;

using Asp.Versioning.Http.UsingMediaType.Controllers;
using System.Web.Http;

public class MediaTypeFixture : HttpServerFixture
{
    public MediaTypeFixture()
    {
        FilteredControllerTypes.Add( typeof( ValuesController ) );
        FilteredControllerTypes.Add( typeof( Values2Controller ) );
        FilteredControllerTypes.Add( typeof( HelloWorldController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ApiVersionReader = new MediaTypeApiVersionReader();
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
        options.ReportApiVersions = true;
    }

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        configuration.MapHttpAttributeRoutes();
    }
}