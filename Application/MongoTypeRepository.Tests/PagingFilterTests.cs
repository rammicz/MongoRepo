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

        [Theory]
        [InlineData(FilterOperator.Contains, "a(b+", @"a\(b\+")] // metacharacters escaped (red spec for #12)
        [InlineData(FilterOperator.Contains, "1.5", @"1\.5")] // dot escaped: literal "1.5" must not match "135"
        [InlineData(FilterOperator.StartsWith, "a(b+", @"^a\(b\+")] // anchored + escaped
        [InlineData(FilterOperator.EndsWidth, "a(b+", @"a\(b\+$")] // anchored + escaped
        [InlineData(FilterOperator.Contains, null, "")] // null degrades to match-anything, no throw (pre-#12 parity)
        public void StringFilter_EscapesValueAndAnchors(FilterOperator op, string value, string expectedPattern)
        {
            var paging = new RepositoryPaging
            {
                Filtering = new List<Filtering>
                {
                    new Filtering { By = "Name", Operator = op, Value = value }
                }
            };
            var filter = TestRepository.BuildPagingFilter(paging);
            var rendered = Render(filter);
            Assert.Equal(expectedPattern, Pattern(rendered, "Name"));
        }
    }
}
