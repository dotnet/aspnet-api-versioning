﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace.Controllers.V1;

using Asp.Versioning.Http.UsingNamespace.Models;
using System.Net.Http;
using System.Web.Http;

public class AgreementsController : ApiController
{
    public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
}