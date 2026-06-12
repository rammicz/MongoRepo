using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTypeRepository.Contracts;
using Xunit;
using static MongoTypeRepository.Tests.RenderHelper;

namespace MongoTypeRepository.Tests
{
    public class PagingFilterTests
    {
        /// <summary>
        /// Green pin test: locks in the current rendered BSON for an Equals filter
        /// on a string value, before the regex-escape fix (#12) touches this method.
        /// </summary>
        [Fact]
        public void EqualsFilter_OnStringValue_RendersExpectedBson()
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = FilterOperator.Equals, Value = "alice" }
                }
            };

            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);

            Assert.Equal(new BsonDocument("Name", "alice"), rendered);
        }

        [Fact(Skip = "red: filter value is interpolated into the regex without Regex.Escape")]
        public void ContainsFilter_EscapesRegexMetacharacters()
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = FilterOperator.Contains, Value = "a(b+" }
                }
            };
            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);
            Assert.Contains(@"a\(b\+", rendered.ToString());
        }
    }
}
