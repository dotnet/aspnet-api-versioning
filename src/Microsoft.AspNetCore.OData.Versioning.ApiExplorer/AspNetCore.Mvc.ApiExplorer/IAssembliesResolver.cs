namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal interface IAssembliesResolver
    {
        IReadOnlyCollection<Assembly> GetAssemblies();
    }
}