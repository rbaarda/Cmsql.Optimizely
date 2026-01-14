using System.Collections.Generic;
using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using EPiServer;
using EPiServer.DataAbstraction;
using FluentAssertions;
using Xunit;

namespace Cmsql.EpiServer.Test.Internal
{
    public class CmsqlBinaryOrExpressionVisitorTest
    {
        [Fact]
        public void Test_when_visit_query_condition_push_new_criteria_collection()
        {
            // Arrange
            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = "PageName",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_when_orexpression_visitor_visits_andexpression_with_conditions_push_one_new_criteria_collection()
        {
            // Arrange
            CmsqlQueryBinaryExpression expression = new CmsqlQueryBinaryExpression
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

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expression);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_when_orexpression_visitor_visits_orexpression_push_criteria_collection_for_every_condition()
        {
            // Arrange
            CmsqlQueryBinaryExpression expression = new CmsqlQueryBinaryExpression
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

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlBinaryOrExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expression);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(2);
        }
    }
}
