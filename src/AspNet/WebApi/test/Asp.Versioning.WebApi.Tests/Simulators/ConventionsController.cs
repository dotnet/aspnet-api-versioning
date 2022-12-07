// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;

public sealed class ConventionsController : ApiController
{
    public string Get() => "Test (1.0)";

    public string Get( int id ) => $"Test {id} (1.0)";

    public string GetV2() => "Test (2.0)";

    public string GetV2( int id ) => $"Test {id} (2.0)";
}