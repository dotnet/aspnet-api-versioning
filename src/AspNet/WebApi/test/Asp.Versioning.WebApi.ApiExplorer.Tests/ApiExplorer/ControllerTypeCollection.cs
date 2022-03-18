// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using System.Collections.ObjectModel;
using System.Web.Http.Dispatcher;

public class ControllerTypeCollection : Collection<Type>, IHttpControllerTypeResolver
{
    public ControllerTypeCollection() { }

    public ControllerTypeCollection( params Type[] controllerTypes ) : base( controllerTypes.ToList() ) { }

    public ICollection<Type> GetControllerTypes( IAssembliesResolver assembliesResolver ) => this;
}