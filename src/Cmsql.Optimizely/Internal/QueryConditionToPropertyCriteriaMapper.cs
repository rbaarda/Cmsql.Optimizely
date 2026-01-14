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

            var compareCondition = MapEqualityOperatorToCompareCondition(condition.Operator);
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

        internal static CompareCondition MapEqualityOperatorToCompareCondition(EqualityOperator operatr)
        {
            return operatr switch
            {
                EqualityOperator.Equals => CompareCondition.Equal,
                EqualityOperator.GreaterThan => CompareCondition.GreaterThan,
                EqualityOperator.LessThan => CompareCondition.LessThan,
                EqualityOperator.NotEquals => CompareCondition.NotEqual,
                _ => throw new InvalidOperationException($"Equality operator '{operatr}' not supported."),
            };
        }
    }
}
