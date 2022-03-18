// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Web.Http.Dispatcher;

internal sealed class FilteredControllerTypes : List<Type>, IHttpControllerTypeResolver
{
    public ICollection<Type> GetControllerTypes( IAssembliesResolver assembliesResolver ) => this;
}