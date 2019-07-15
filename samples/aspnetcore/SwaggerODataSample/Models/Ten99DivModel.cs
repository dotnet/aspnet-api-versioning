using System.Runtime.Serialization;

namespace Microsoft.Examples.Models
{
    [DataContract]
    public class Ten99DivModel
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
        public string TrustNo { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        [DataMember]
        public int TrustKey { get; set; }
        /// <summary>
        /// Tax year and sequence
        /// </summary>
        [DataMember( Name = "yr_seq" )]
        public short YrSeq { get; set; }
        /// <summary>
        /// Account
        /// </summary>
        [DataMember( Name = "account" )]
        public TrustModel Account { get; set; }

    }
}