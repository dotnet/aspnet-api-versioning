namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Http.Dispatcher;

    public class ControllerTypeCollection : Collection<Type>, IHttpControllerTypeResolver
    {
        public ControllerTypeCollection() { }

        public ControllerTypeCollection( params Type[] controllerTypes ) : base( controllerTypes.ToList() ) { }

        public ICollection<Type> GetControllerTypes( IAssembliesResolver assembliesResolver ) => this;
    }
}