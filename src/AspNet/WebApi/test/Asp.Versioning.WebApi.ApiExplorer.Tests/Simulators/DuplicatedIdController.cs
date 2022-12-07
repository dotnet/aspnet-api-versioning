// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Simulators;

using Asp.Versioning.Models;
using System.Web.Http;

public class DuplicatedIdController : ApiController
{
    public void Get( [FromUri] ClassWithId objectWithId ) { }
}