using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using EPiServer.DataAbstraction;
using FluentAssertions;
using Xunit;

namespace Cmsql.Optimizely.Test.Internal
{
    public class CmsqlBinaryOrExpressionVisitorTest
    {
        [Fact]
        public void Test_when_visit_query_condition_push_new_criteria_collection()
        {
            // Arrange
            var condition = new CmsqlQueryCondition
            {
                Identifier = "PageName",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_when_orexpression_visitor_visits_andexpression_with_conditions_push_one_new_criteria_collection()
        {
            // Arrange
            var expression = new CmsqlQueryBinaryExpression
            {
                Operator = ConditionalOperator.And,
                LeftExpression = new CmsqlQueryCondition
                {
                    Identifier = "PageName",
                    Operator = EqualityOperator.GreaterThan,
                    Value = "5"
                },
                RightExpression = new CmsqlQueryCondition
                {
                    Identifier = "PageName",
                    Operator = EqualityOperator.GreaterThan,
                    Value = "5"
                }
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expression);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_when_orexpression_visitor_visits_orexpression_push_criteria_collection_for_every_condition()
        {
            // Arrange
            var expression = new CmsqlQueryBinaryExpression
            {
                Operator = ConditionalOperator.Or,
                LeftExpression = new CmsqlQueryCondition
                {
                    Identifier = "PageName",
                    Operator = EqualityOperator.GreaterThan,
                    Value = "5"
                },
                RightExpression = new CmsqlQueryCondition
                {
                    Identifier = "PageName",
                    Operator = EqualityOperator.GreaterThan,
                    Value = "5"
                }
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expression);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(2);
        }
    }
}
