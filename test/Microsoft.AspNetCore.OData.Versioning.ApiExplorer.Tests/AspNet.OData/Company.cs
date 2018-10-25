namespace Microsoft.AspNet.OData
{
    using System;
    using System.Collections.Generic;

    public class Company
    {
        public int CompanyId { get; set; }

        public Company ParentCompany { get; set; }

        public List<Company> Subsidiaries { get; set; }

        public string Name { get; set; }

        public DateTime DateFounded { get; set; }
    }
}