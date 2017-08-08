using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MongoTypeRepository.Contracts
{
    [DataContract]
    public class RepositoryPaging
    {
        [DataMember]
        public int CurrentPage { get; set; } = 1;

        [DataMember]
        public int PageSize { get; set; } = 100;

        [DataMember]
        public long TotalItems { get; set; } = -1;

        [DataMember]
        public string OrderBy { get; set; }

        [DataMember]
        public Ordering OrderDirection { get; set; }

        /// <summary>
        ///     This is meant to be additional filtering for UI needs. this filter apply after all business filters are applied
        /// </summary>
        [DataMember]
        public List<Filtering> Filtering { get; set; }
    }
}