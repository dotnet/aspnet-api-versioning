namespace Microsoft.AspNet.OData
{
    public class Company
    {
        public int CompanyId { get; set; }
        
        public Company ParentCompany { get; set; }
        
        public string Name { get; set; }
        
    }
}