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
        /// Pulls the raw regex pattern string out of a rendered { field: { $regularExpression: { pattern, options } } }
        /// document. Asserting on the pattern value directly avoids the JSON double-escaping that
        /// <see cref="BsonDocument.ToString"/> applies to backslashes.
        /// </summary>
        private static string Pattern(BsonDocument rendered, string field) =>
            rendered[field].AsBsonRegularExpression.Pattern;

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

        [Fact]
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
            Assert.Equal(@"a\(b\+", Pattern(rendered, "Name"));
        }

        [Fact]
        public void ContainsFilter_EscapesDot_SoLiteralMatchOnly()
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = FilterOperator.Contains, Value = "1.5" }
                }
            };
            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);
            Assert.Equal(@"1\.5", Pattern(rendered, "Name"));
        }

        [Fact]
        public void StartsWithFilter_AnchorsAndEscapes()
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = FilterOperator.StartsWith, Value = "a(b+" }
                }
            };
            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);
            Assert.Equal(@"^a\(b\+", Pattern(rendered, "Name"));
        }

        [Fact]
        public void EndsWithFilter_AnchorsAndEscapes()
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = FilterOperator.EndsWidth, Value = "a(b+" }
                }
            };
            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);
            Assert.Equal(@"a\(b\+$", Pattern(rendered, "Name"));
        }
    }
}
