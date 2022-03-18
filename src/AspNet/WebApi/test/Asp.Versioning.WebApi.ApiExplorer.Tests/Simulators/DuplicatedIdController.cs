// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060
#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using Asp.Versioning.Models;
using System.Web.Http;

public class DuplicatedIdController : ApiController
{
    public void Get( [FromUri] ClassWithId objectWithId ) { }
}