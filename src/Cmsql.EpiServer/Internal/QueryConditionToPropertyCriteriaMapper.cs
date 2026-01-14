using System;
using Cmsql.Query;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;

namespace Cmsql.Optimizely.Internal
{
    internal class QueryConditionToPropertyCriteriaMapper
    {
        private readonly PropertyDataTypeResolver _propertyDataTypeResolver;

        public QueryConditionToPropertyCriteriaMapper(PropertyDataTypeResolver propertyDataTypeResolver)
        {
            _propertyDataTypeResolver = propertyDataTypeResolver;
        }

        internal bool TryMap(CmsqlQueryCondition condition, out PropertyCriteria criteria)
        {
            criteria = null;

            if (condition == null)
            {
                return false;
            }

            if (!_propertyDataTypeResolver.TryResolve(condition.Identifier, out PropertyDataType propertyDataType))
            {
                return false;
            }

            CompareCondition compareCondition = MapEqualityOperatorToCompareCondition(condition.Operator);
            criteria = new PropertyCriteria
            {
                Condition = compareCondition,
                Value = condition.Value,
                Name = condition.Identifier,
                Type = propertyDataType,
                Required = true
            };

            return true;
        }

        internal CompareCondition MapEqualityOperatorToCompareCondition(EqualityOperator operatr)
        {
            switch (operatr)
            {
                case EqualityOperator.Equals:
                    return CompareCondition.Equal;
                case EqualityOperator.GreaterThan:
                    return CompareCondition.GreaterThan;
                case EqualityOperator.LessThan:
                    return CompareCondition.LessThan;
                case EqualityOperator.NotEquals:
                    return CompareCondition.NotEqual;
            }

            throw new InvalidOperationException($"Equality operator '{operatr}' not supported.");
        }
    }
}
