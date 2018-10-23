namespace Microsoft.AspNet.OData
{
    using System.Collections.Generic;

    public class Contact
    {
        public int ContactId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public List<Address> Addresses { get; set; }
    }
}