using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Examples.Models
{
    [DataContract]
    public class Ten99BModel
    {
        /// <summary>
        /// Product Access Number
        /// </summary>
        [DataMember( Name = "pan" )]
        public string Pan { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        [DataMember( Name = "account_number" )]
        //[DataMember]
        public string TrustNo { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        [DataMember( Name = "key_trust")]
        public int TrustKey { get; set; }

        /// <summary>
        /// Tax year and sequence
        /// </summary>
        [DataMember( Name = "yr_seq" )]
        //[DataMember]
        public short YrSeq { get; set; }

        /// <summary>
        /// Account
        /// </summary>
        [DataMember( Name = "account_trust" )]
        //[DataMember]
        //[Contained]
        public TrustModel Account { get; set; }

    }
}