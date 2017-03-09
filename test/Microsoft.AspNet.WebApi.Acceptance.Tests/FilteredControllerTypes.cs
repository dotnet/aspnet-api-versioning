namespace Microsoft.Web
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dispatcher;

    sealed class FilteredControllerTypes : List<Type>, IHttpControllerTypeResolver
    {
        public ICollection<Type> GetControllerTypes( IAssembliesResolver assembliesResolver ) => this;
    }
}