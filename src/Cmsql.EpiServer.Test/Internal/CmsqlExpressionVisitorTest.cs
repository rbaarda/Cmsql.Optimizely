using System.Collections.Generic;
using System.Linq;
using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using Cmsql.Query.Execution;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using FluentAssertions;
using Xunit;

namespace Cmsql.Optimizely.Test.Internal
{
    public class CmsqlExpressionVisitorTest
    {
        [Fact]
        public void Test_can_map_query_condition_to_property_criteria()
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
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            PropertyCriteriaCollection propertyCriteriaCollection = context.GetCriteria().Single();

            PropertyCriteria propertyCriteria = propertyCriteriaCollection.Last();

            // Assert
            propertyCriteria.Value.Should().BeEquivalentTo(condition.Value);
            propertyCriteria.Condition.Should().Be(CompareCondition.GreaterThan);
            propertyCriteria.Name.Should().BeEquivalentTo(condition.Identifier);
        }

        [Fact]
        public void Test_when_condition_is_null_criteria_should_be_empty()
        {
            // Arrange
            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(null);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();
            
            // Assert
            propertyCriteriaCollection.Should().BeEmpty();
        }

        [Fact]
        public void Test_when_condition_is_null_context_should_contain_error()
        {
            // Arrange
            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(null);

            // Assert
            CmsqlQueryExecutionError error = context.Errors.Single();
            error.Message.Should().BeEquivalentTo("Could not process malformed query condition.");
        }

        [Fact]
        public void Test_when_property_cannot_be_resolved_context_should_contain_error()
        {
            // Arrange
            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = "ThisPropertyCannotBeFound",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);
            
            // Assert
            CmsqlQueryExecutionError error = context.Errors.Single();
            error.Message.Should().BeEquivalentTo("Could not find property 'ThisPropertyCannotBeFound'");
        }

        [Fact]
        public void Test_when_condition_cannot_be_mapped_criteria_should_be_empty()
        {
            // Arrange
            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = "ThisPropertyCannotBeFound",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryCondition(condition);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().BeEmpty();
        }

        [Fact]
        public void Test_can_map_binary_orexpression_to_two_criteria_collections()
        {
            // Arrange
            CmsqlQueryBinaryExpression orExpression = new CmsqlQueryBinaryExpression
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

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(orExpression);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(2);
        }

        [Fact]
        public void Test_can_map_binary_andexpression_to_one_criteria_collections()
        {
            // Arrange
            CmsqlQueryBinaryExpression orExpression = new CmsqlQueryBinaryExpression
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

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(orExpression);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(1);
        }

        [Fact]
        public void Test_can_map_nested_expressions()
        {
            // Arrange
            CmsqlQueryBinaryExpression expressions = new CmsqlQueryBinaryExpression
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

            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            CmsqlExpressionVisitor cmsqlExpressionVisitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(new ContentType())), context);

            // Act
            cmsqlExpressionVisitor.VisitQueryExpression(expressions);

            IEnumerable<PropertyCriteriaCollection> propertyCriteriaCollection = context.GetCriteria();

            // Assert
            propertyCriteriaCollection.Should().HaveCount(6);
        }
    }
}
