// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.Controllers;

public abstract class ODataFixture : HttpServerFixture
{
    protected ODataFixture() => FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );
}