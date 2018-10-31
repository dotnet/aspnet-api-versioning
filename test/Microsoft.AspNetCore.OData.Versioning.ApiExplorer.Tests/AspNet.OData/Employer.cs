namespace Microsoft.AspNet.OData
{
    using System;
    using System.Collections.Generic;

    public class Employer
    {
        public int EmployerId { get; set; }

        public ICollection<Employee> Employees { get; set; }
        
        public DateTime Birthday { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}