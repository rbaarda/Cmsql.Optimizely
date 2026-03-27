using System.Linq;
using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using EPiServer.DataAbstraction;
using FluentAssertions;
using Xunit;

namespace Cmsql.Optimizely.Test.Internal
{
    public class CmsqlExpressionParserTest
    {
        [Fact]
        public void Test_when_content_type_is_null_result_should_have_no_criteria()
        {
            // Arrange
            var parser = new CmsqlExpressionParser();

            // Act
            var context = parser.Parse(null!, null!);

            // Assert
            context.GetCriteria().Should().BeEmpty();
            context.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Test_when_expression_is_null_result_should_have_one_criteria_collection()
        {
            // Arrange
            var parser = new CmsqlExpressionParser();
            var contentType = new ContentType { ID = 1 };

            // Act
            var context = parser.Parse(contentType, null!);

            // Assert
            context.GetCriteria().Should().HaveCount(1);
            context.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Test_when_expression_is_valid_result_should_contain_mapped_criteria()
        {
            // Arrange
            var parser = new CmsqlExpressionParser();
            var contentType = new ContentType { ID = 1 };
            var expression = new CmsqlQueryCondition
            {
                Identifier = MetaDataProperties.PageName,
                Operator = EqualityOperator.Equals,
                Value = "Test"
            };

            // Act
            var context = parser.Parse(contentType, expression);

            // Assert
            context.Errors.Should().BeEmpty();
            var criteria = context.GetCriteria().Should().HaveCount(1).And.Subject.Single();
            criteria.Should().HaveCount(2); // PageType filter + the condition
        }
    }
}
