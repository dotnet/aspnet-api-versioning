namespace Microsoft.AspNet.OData
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        
        public Employer Employer { get; set; }

        public decimal Salary { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}