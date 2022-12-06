// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Runtime.CompilerServices;
using System.Web.Http.Description;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Web API.
/// </content>
public partial class ODataQueryOptionsConventionBuilder
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type GetController( ApiDescription apiDescription ) =>
        apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
}