// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Advanced.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;

[ApiVersion( "2.0" )]
[ControllerName( "Orders" )]
[ODataRoutePrefix( "Orders" )]
public class Orders2Controller : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
        Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

    [ODataRoute( "{key}" )]
    public IHttpActionResult Get( int key, ODataQueryOptions<Order> options ) =>
        Ok( new Order() { Id = key, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
}