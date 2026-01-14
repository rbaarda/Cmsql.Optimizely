using Cmsql.Query;
using EPiServer.DataAbstraction;

namespace Cmsql.Optimizely.Internal
{
    internal class CmsqlExpressionParser
    {
        public CmsqlExpressionVisitorContext Parse(
            ContentType contentType,
            ICmsqlQueryExpression expression)
        {
            var context = new CmsqlExpressionVisitorContext(contentType);

            if (contentType == null)
            {
                return context;
            }

            if (expression == null)
            {
                context.PushNewPropertyCriteriaCollection();
                return context;
            }
            
            var visitor =
                new CmsqlExpressionVisitor(
                    new QueryConditionToPropertyCriteriaMapper(
                        new PropertyDataTypeResolver(contentType)), context);

            expression.Accept(visitor);

            return context;
        }
    }
}
