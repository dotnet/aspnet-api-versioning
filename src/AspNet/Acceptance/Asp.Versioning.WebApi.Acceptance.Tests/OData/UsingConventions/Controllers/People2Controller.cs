// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.UsingConventions.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using System.Web.Http;

[ControllerName( "People" )]
public class People2Controller : ODataController
{
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

    public IHttpActionResult Get( int key, ODataQueryOptions<Person> options ) =>
        Ok( new Person() { Id = key, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );
}