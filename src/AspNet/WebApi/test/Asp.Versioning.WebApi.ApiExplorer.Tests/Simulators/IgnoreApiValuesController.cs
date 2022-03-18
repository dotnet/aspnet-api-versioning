// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using System.Web.Http;
using System.Web.Http.Description;

[ApiExplorerSettings( IgnoreApi = true )]
public class IgnoreApiValuesController : ApiController
{
    public void Get() { }

    public void Post() { }
}