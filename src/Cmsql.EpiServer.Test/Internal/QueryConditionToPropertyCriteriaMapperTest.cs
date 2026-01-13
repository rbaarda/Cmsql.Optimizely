using System;
using Cmsql.EpiServer.Internal;
using Cmsql.Query;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using FluentAssertions;
using Xunit;

namespace Cmsql.EpiServer.Test.Internal
{
    public class QueryConditionToPropertyCriteriaMapperTest
    {
        [Fact]
        public void Test_can_map_query_condition_to_property_criteria()
        {
            // Arrange
            ContentType contentType = new ContentType
            {
                PropertyDefinitions =
                {
                    new PropertyDefinition
                    {
                        Name = "FooBar",
                        Type = new PropertyDefinitionType
                        {
                            DataType = PropertyDataType.Number
                        }
                    }
                }
            };

            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = "FooBar",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            QueryConditionToPropertyCriteriaMapper mapper = new QueryConditionToPropertyCriteriaMapper(
                new PropertyDataTypeResolver(contentType));

            // Act
            bool isMapSuccessfull = mapper.TryMap(condition, out PropertyCriteria criteria);

            // Assert
            isMapSuccessfull.Should().BeTrue();
            criteria.Name.Should().BeEquivalentTo(condition.Identifier);
            criteria.Condition.Should().Be(CompareCondition.GreaterThan);
            criteria.Value.Should().BeEquivalentTo(condition.Value);
            criteria.Type.Should().Be(PropertyDataType.Number);
        }

        [Fact]
        public void Test_can_map_query_condition_with_meta_data_property_to_property_criteria()
        {
            // Arrange
            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = MetaDataProperties.PageName,
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            QueryConditionToPropertyCriteriaMapper mapper = new QueryConditionToPropertyCriteriaMapper(
                new PropertyDataTypeResolver(new ContentType()));

            // Act
            bool isMapSuccessfull = mapper.TryMap(condition, out PropertyCriteria criteria);

            // Assert
            isMapSuccessfull.Should().BeTrue();
            criteria.Name.Should().BeEquivalentTo(condition.Identifier);
            criteria.Condition.Should().Be(CompareCondition.GreaterThan);
            criteria.Value.Should().BeEquivalentTo(condition.Value);
            criteria.Type.Should().Be(PropertyDataType.String);
        }

        [Fact]
        public void Test_when_condition_is_null_mapping_should_return_false()
        {
            // Arrange
            QueryConditionToPropertyCriteriaMapper mapper = new QueryConditionToPropertyCriteriaMapper(
                new PropertyDataTypeResolver(new ContentType()));

            // Act
            bool isMapSuccessfull = mapper.TryMap(null, out PropertyCriteria criteria);

            // Assert
            isMapSuccessfull.Should().BeFalse();
            criteria.Should().BeNull();
        }

        [Fact]
        public void Test_when_property_is_unkown_mapping_should_return_false()
        {
            // Arrange
            CmsqlQueryCondition condition = new CmsqlQueryCondition
            {
                Identifier = "This is some unknown property",
                Operator = EqualityOperator.GreaterThan,
                Value = "5"
            };

            QueryConditionToPropertyCriteriaMapper mapper = new QueryConditionToPropertyCriteriaMapper(
                new PropertyDataTypeResolver(new ContentType()));

            // Act
            bool isMapSuccessfull = mapper.TryMap(condition, out PropertyCriteria criteria);

            // Assert
            isMapSuccessfull.Should().BeFalse();
            criteria.Should().BeNull();
        }

        [Theory]
        [InlineData(EqualityOperator.GreaterThan, CompareCondition.GreaterThan)]
        [InlineData(EqualityOperator.Equals, CompareCondition.Equal)]
        [InlineData(EqualityOperator.LessThan, CompareCondition.LessThan)]
        [InlineData(EqualityOperator.NotEquals, CompareCondition.NotEqual)]
        public void Test_can_map_equality_operator_to_compare_condition(EqualityOperator operatr, CompareCondition condition)
        {
            QueryConditionToPropertyCriteriaMapper mapper =
                new QueryConditionToPropertyCriteriaMapper(
                    new PropertyDataTypeResolver(new ContentType()));

            CompareCondition mappedCondition = mapper.MapEqualityOperatorToCompareCondition(operatr);

            mappedCondition.Should().Be(condition);
        }

        [Fact]
        public void Test_when_mapping_unknown_equality_operator_throw()
        {
            QueryConditionToPropertyCriteriaMapper mapper =
                new QueryConditionToPropertyCriteriaMapper(
                    new PropertyDataTypeResolver(new ContentType()));

            mapper.Invoking(m => m.MapEqualityOperatorToCompareCondition(EqualityOperator.None))
                .Should().Throw<InvalidOperationException>();
        }
    }
}
