namespace Microsoft.AspNet.OData
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Contact
    {
        [DataMember]
        public int ContactId { get; set; }

        [DataMember( Name = "first_name" )]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public Email Email { get; set; }

        [DataMember( Name = "telephone" )]
        public string Phone { get; set; }

        [DataMember]
        public List<Address> Addresses { get; set; }
    }
}