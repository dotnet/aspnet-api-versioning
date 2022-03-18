// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.OData;
using System.Web.Http.Dependencies;

internal static class DependencyResolverExtensions
{
    internal static TService? GetService<TService>( this IDependencyResolver resolver ) => (TService) resolver.GetService( typeof( TService ) );

    internal static IModelTypeBuilder GetModelTypeBuilder( this IDependencyResolver resolver ) =>
        resolver.GetService<IModelTypeBuilder>() ?? new DefaultModelTypeBuilder();
}