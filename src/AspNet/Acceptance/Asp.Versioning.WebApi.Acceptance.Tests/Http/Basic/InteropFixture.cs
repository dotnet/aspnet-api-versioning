// Copyright (c) .NET Foundation and contributors. All rights reserved.

// Ignore Spelling: Interop
namespace Asp.Versioning.Http.Basic;

using System.Web.Http;

public class InteropFixture : BasicFixture
{
    protected override void OnConfigure( HttpConfiguration configuration )
    {
        configuration.ConvertProblemDetailsToErrorObject();
        base.OnConfigure( configuration );
    }
}