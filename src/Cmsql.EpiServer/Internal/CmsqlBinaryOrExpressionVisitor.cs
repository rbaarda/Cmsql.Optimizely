using Cmsql.Query;

namespace Cmsql.Optimizely.Internal
{
    internal class CmsqlBinaryOrExpressionVisitor : CmsqlExpressionVisitor
    {
        internal CmsqlBinaryOrExpressionVisitor(
            QueryConditionToPropertyCriteriaMapper conditionToCriteriaMapper,
            CmsqlExpressionVisitorContext context)
            : base(conditionToCriteriaMapper, context)
        {
        }

        public override void VisitQueryCondition(CmsqlQueryCondition condition)
        {
            Context.PushNewPropertyCriteriaCollection();

            base.VisitQueryCondition(condition);
        }

        public override void VisitQueryExpression(CmsqlQueryBinaryExpression binaryExpression)
        {
            if (binaryExpression.Operator == ConditionalOperator.And)
            {
                Context.PushNewPropertyCriteriaCollection();
            }

            base.VisitQueryExpression(binaryExpression);
        }
    }
}
