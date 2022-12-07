// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;

public class AttributeApiExplorerValuesController : ApiController
{
    [Route( "" )]
    [HttpGet]
    public void Action() { }
}