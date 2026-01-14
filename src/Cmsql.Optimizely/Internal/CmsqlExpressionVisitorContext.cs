using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cmsql.Query.Execution;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;

namespace Cmsql.Optimizely.Internal
{
    internal class CmsqlExpressionVisitorContext
    {
        private readonly Stack<PropertyCriteriaCollection> _propertyCriteriaCollectionStack;
        private readonly ContentType _contentType;

        internal IList<CmsqlQueryExecutionError> Errors { get; }

        internal CmsqlExpressionVisitorContext(ContentType contentType)
        {
            _propertyCriteriaCollectionStack = new Stack<PropertyCriteriaCollection>();
            _contentType = contentType;

            Errors = new List<CmsqlQueryExecutionError>();
        }

        internal void AddPropertyCriteria(PropertyCriteria propertyCriteria)
        {
            Debug.Assert(propertyCriteria != null);

            if (!_propertyCriteriaCollectionStack.Any())
            {
                PushNewPropertyCriteriaCollection();
            }

            _propertyCriteriaCollectionStack.Peek().Add(propertyCriteria);
        }

        internal void PushNewPropertyCriteriaCollection()
        {
            _propertyCriteriaCollectionStack.Push(new PropertyCriteriaCollection());

            // TODO: Review this, not sure if this belongs here.
            AddPropertyCriteria(new PropertyCriteria
            {
                Condition = CompareCondition.Equal,
                IsNull = false,
                Name = MetaDataProperties.PageTypeID,
                Required = true,
                Type = PropertyDataType.PageType,
                Value = _contentType.ID.ToString()
            });
        }

        internal IEnumerable<PropertyCriteriaCollection> GetCriteria()
        {
            return _propertyCriteriaCollectionStack;
        }
    }
}
