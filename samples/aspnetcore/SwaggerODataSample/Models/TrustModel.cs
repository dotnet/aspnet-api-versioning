using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.Examples.Models
{
    //[Select]
    [DataContract]
    public class TrustModel
    {
        /// <summary>
        /// Key
        /// </summary>
        [DataMember]
        public int TrustKey { get; set; }
        /// <summary>
        /// Perm sequence
        /// </summary>
        [DataMember]
        public byte PermSeq { get; set; }
        /// <summary>
        /// Tax year and sequence
        /// </summary>
        [DataMember( Name = "yr_seq" )]
        public short YrSeq { get; set; }
        /// <summary>
        /// Account details
        /// </summary>
        [DataMember( Name = "account_details" )]
        //[DataMember]
        //[Contained]
        public AccountDetails Details { get; set; }
    }
}