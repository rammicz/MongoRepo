using System.Runtime.Serialization;

namespace MongoTypeRepository.Contracts
{
    [DataContract]
    public enum Ordering
    {
        [EnumMember]
        asc,

        [EnumMember]
        desc
    }
}