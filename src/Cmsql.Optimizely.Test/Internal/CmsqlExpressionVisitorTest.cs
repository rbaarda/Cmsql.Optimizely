using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Cmsql.Optimizely.Test.Internal
{
    public class CmsqlExpressionVisitorTest
    {
        [Fact]
        public void Test_can_map_query_condition_to_property_criteria()
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
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            var propertyCriteriaCollection = context.GetCriteria().Single();

            var propertyCriteria = propertyCriteriaCollection.Last();

            // Assert
            propertyCriteria.Value.Should().BeEquivalentTo(condition.Value);
            propertyCriteria.Condition.Should().Be(CompareCondition.GreaterThan);
            propertyCriteria.Name.Should().BeEquivalentTo(condition.Identifier);
        }

        [Fact]
        public void Test_when_condition_is_null_criteria_should_be_empty()
        {
            // Arrange
            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(null);

            var propertyCriteriaCollection = context.GetCriteria();
            
            // Assert
            propertyCriteriaCollection.Should().BeEmpty();
        }

        [Fact]
        public void Test_when_condition_is_null_context_should_contain_error()
        {
            // Arrange
            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(null);

            // Assert
            var error = context.Errors.Single();
            error.Message.Should().BeEquivalentTo("Could not process malformed query condition.");
        }

        [Fact]
        public void Test_when_property_cannot_be_resolved_context_should_contain_error()
        {
            // Arrange
            var condition = new CmsqlQueryCondition
            {
                Identifier = "ThisPropertyCannotBeFound",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);
            
            // Assert
            var error = context.Errors.Single();
            error.Message.Should().BeEquivalentTo("Could not find property 'ThisPropertyCannotBeFound'");
        }

        [Fact]
        public void Test_when_condition_cannot_be_mapped_criteria_should_be_empty()
        {
            // Arrange
            var condition = new CmsqlQueryCondition
            {
                Identifier = "ThisPropertyCannotBeFound",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().BeEmpty();
        }

        [Fact]
        public void Test_can_map_binary_orexpression_to_two_criteria_collections()
        {
            // Arrange
            var orExpression = new CmsqlQueryBinaryExpression
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
                    Operator = EqualityOperator.Equals,
                    Value = "10"
                }
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(orExpression);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(2);
        }

        [Fact]
        public void Test_can_map_binary_andexpression_to_one_criteria_collections()
        {
            // Arrange
            var orExpression = new CmsqlQueryBinaryExpression
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
                    Operator = EqualityOperator.Equals,
                    Value = "10"
                }
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(orExpression);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_can_map_nested_expressions()
        {
            // Arrange
            var expressions = new CmsqlQueryBinaryExpression
            {
                Operator = ConditionalOperator.Or,
                LeftExpression = new CmsqlQueryBinaryExpression
                {
                    Operator = ConditionalOperator.Or,
                    LeftExpression = new CmsqlQueryBinaryExpression
                    {
                        Operator = ConditionalOperator.Or,
                        LeftExpression = new CmsqlQueryCondition
                        {
                            Identifier = "PageName",
                            Operator = EqualityOperator.Equals,
                            Value = "1"
                        },
                        RightExpression = new CmsqlQueryBinaryExpression
                        {
                            Operator = ConditionalOperator.Or,
                            LeftExpression = new CmsqlQueryCondition
                            {
                                Identifier = "PageName",
                                Operator = EqualityOperator.Equals,
                                Value = "2"
                            },
                            RightExpression = new CmsqlQueryCondition
                            {
                                Identifier = "PageName",
                                Operator = EqualityOperator.Equals,
                                Value = "3"
                            }
                        }
                    },
                    RightExpression = new CmsqlQueryBinaryExpression
                    {
                        Operator = ConditionalOperator.Or,
                        LeftExpression = new CmsqlQueryCondition
                        {
                            Identifier = "PageName",
                            Operator = EqualityOperator.Equals,
                            Value = "4"
                        },
                        RightExpression = new CmsqlQueryCondition
                        {
                            Identifier = "PageName",
                            Operator = EqualityOperator.Equals,
                            Value = "5"
                        }
                    }
                },
                RightExpression = new CmsqlQueryBinaryExpression
                {
                    Operator = ConditionalOperator.And,
                    RightExpression = new CmsqlQueryCondition
                    {
                        Identifier = "PageName",
                        Operator = EqualityOperator.Equals,
                        Value = "6"
                    },
                    LeftExpression = new CmsqlQueryCondition
                    {
                        Identifier = "PageName",
                        Operator = EqualityOperator.Equals,
                        Value = "7"
                    }
                }
            };

            var context = new CmsqlExpressionVisitorContext(new ContentType());

            var cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expressions);

            var propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(6);
        }
    }
}
