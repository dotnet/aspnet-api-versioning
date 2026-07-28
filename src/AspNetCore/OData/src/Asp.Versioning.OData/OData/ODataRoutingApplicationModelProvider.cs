// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.UnsafeAccessorKind;

internal static class ODataRoutingApplicationModelProvider
{
    private const string TypeName = "Microsoft.AspNetCore.OData.Routing.ODataRoutingApplicationModelProvider, Microsoft.AspNetCore.OData";

    public static IApplicationModelProvider New() => New( Options.Create( new ODataOptions() ) );

    public static IApplicationModelProvider New( IOptions<ODataOptions> options ) => (IApplicationModelProvider) Ctor( options );

    public static new Type GetType() => Type.GetType( TypeName, throwOnError: true )!;

    [UnsafeAccessor( Constructor )]
    [return: UnsafeAccessorType( TypeName )]
    private static extern object Ctor( IOptions<ODataOptions> options );
}