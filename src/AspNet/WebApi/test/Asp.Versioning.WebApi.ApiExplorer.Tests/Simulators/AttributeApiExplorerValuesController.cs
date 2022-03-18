// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using System.Web.Http;

public class AttributeApiExplorerValuesController : ApiController
{
    [Route( "" )]
    [HttpGet]
    public void Action() { }
}