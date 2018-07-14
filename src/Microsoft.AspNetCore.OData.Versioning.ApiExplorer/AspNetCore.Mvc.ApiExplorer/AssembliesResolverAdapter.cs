namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal class AssembliesResolverAdapter : IAssembliesResolver
    {
        readonly List<Assembly> assemblies = new List<Assembly>();

        public AssembliesResolverAdapter( ApplicationPartManager partManager )
        {
            Contract.Requires( partManager != null );

            foreach ( var part in partManager.ApplicationParts.OfType<AssemblyPart>() )
            {
                assemblies.Add( part.Assembly );
            }
        }

        public IReadOnlyCollection<Assembly> GetAssemblies() => assemblies;
    }
}