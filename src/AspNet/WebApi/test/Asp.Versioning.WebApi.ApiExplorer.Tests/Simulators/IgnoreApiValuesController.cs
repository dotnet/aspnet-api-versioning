// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;
using System.Web.Http.Description;

[ApiExplorerSettings( IgnoreApi = true )]
public class IgnoreApiValuesController : ApiController
{
    public void Get() { }

    public void Post() { }
}