namespace MongoRepository.Contracts
{
    public class RepositoryPaging
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public long TotalItems { get; set; } = -1;
    }
}