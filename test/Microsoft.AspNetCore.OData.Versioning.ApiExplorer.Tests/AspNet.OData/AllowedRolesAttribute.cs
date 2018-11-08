namespace Microsoft.AspNet.OData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedRolesAttribute : Attribute
    {

        public AllowedRolesAttribute( params string[] parameters)
        {
            AllowedRoles = parameters.ToList();
        }
        
        public List<string> AllowedRoles { get; }
    }
}