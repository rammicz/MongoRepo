using System.Runtime.Serialization;

namespace MongoTypeRepository.Contracts
{
    [DataContract]
    public class Filtering
    {
        [DataMember]
        public string By { get; set; }

        [DataMember]
        public FilterOperator Operator { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}