// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.OData;
using System.Web.Http.Dependencies;

internal static class DependencyResolverExtensions
{
    extension( IDependencyResolver resolver )
    {
        internal TService? GetService<TService>() => (TService) resolver.GetService( typeof( TService ) );

        internal IModelTypeBuilder ModelTypeBuilder =>
            resolver.GetService<IModelTypeBuilder>() ?? new DefaultModelTypeBuilder();
    }
}