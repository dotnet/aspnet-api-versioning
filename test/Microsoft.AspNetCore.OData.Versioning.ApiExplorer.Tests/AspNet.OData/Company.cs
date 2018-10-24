using System;

namespace Microsoft.AspNet.OData
{
    public class Company
    {
        public int CompanyId { get; set; }
        
        public Company ParentCompany { get; set; }
        
        public String Name { get; set; }
        
    }
}