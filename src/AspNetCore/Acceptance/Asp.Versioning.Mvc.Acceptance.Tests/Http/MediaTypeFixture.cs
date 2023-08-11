// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class MediaTypeFixture : MinimalApiFixture
{
    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ApiVersionReader = new MediaTypeApiVersionReader();
}