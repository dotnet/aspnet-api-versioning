using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.Examples.Models
{
    //[Select]
    [DataContract]
    public class AccountDetails
    {
        [DataMember]
        public int TrustKey { get; set; }
        [DataMember]
        public byte PermSeq { get; set; }
        /// <summary>
        /// Taxpayer name
        /// </summary>
        [DataMember( Name = "name_1" )]
        public string Name1 { get; set; }
    }
}