using Cmsql.Query;
using Cmsql.Query.Execution;
using EPiServer;

namespace Cmsql.Optimizely.Internal
{
    internal class CmsqlExpressionVisitor : ICmsqlQueryExpressionVisitor
    {
        private readonly QueryConditionToPropertyCriteriaMapper _conditionToCriteriaMapper;

        protected readonly CmsqlExpressionVisitorContext Context;

        internal CmsqlExpressionVisitor(
            QueryConditionToPropertyCriteriaMapper conditionToCriteriaMapper,
            CmsqlExpressionVisitorContext context)
        {
            _conditionToCriteriaMapper = conditionToCriteriaMapper;
            Context = context;
        }

        public virtual void VisitQueryCondition(CmsqlQueryCondition condition)
        {
            if (condition == null)
            {
                Context.Errors.Add(new CmsqlQueryExecutionError("Could not process malformed query condition."));
                return;
            }

            if (_conditionToCriteriaMapper.TryMap(condition, out PropertyCriteria criteria))
            {
                Context.AddPropertyCriteria(criteria);
            }
            else
            {
                Context.Errors.Add(new CmsqlQueryExecutionError($"Could not find property '{condition.Identifier}'"));
            }
        }

        public virtual void VisitQueryExpression(CmsqlQueryBinaryExpression binaryExpression)
        {
            var visitor = binaryExpression.Operator == ConditionalOperator.Or
                ? new CmsqlBinaryOrExpressionVisitor(_conditionToCriteriaMapper, Context)
                : new CmsqlExpressionVisitor(_conditionToCriteriaMapper, Context);

            binaryExpression.LeftExpression.Accept(visitor);
            binaryExpression.RightExpression.Accept(visitor);
        }
    }
}
