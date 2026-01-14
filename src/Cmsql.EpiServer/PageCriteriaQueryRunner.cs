using Cmsql.Optimizely.Internal;
using Cmsql.Query;
using Cmsql.Query.Execution;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Cmsql.Optimizely.Test")]

namespace Cmsql.Optimizely
{
    public class PageCriteriaQueryRunner : ICmsqlQueryRunner
    {
        private readonly IPageCriteriaQueryService _pageCriteriaQueryService;
        private readonly IContentTypeRepository _contentTypeRepository;

        public PageCriteriaQueryRunner(
            IPageCriteriaQueryService pageCriteriaQueryService,
            IContentTypeRepository contentTypeRepository)
        {
            _pageCriteriaQueryService = pageCriteriaQueryService;
            _contentTypeRepository = contentTypeRepository;
        }

        public CmsqlQueryExecutionResult ExecuteQueries(IEnumerable<CmsqlQuery> queries)
        {
            var errors = new List<CmsqlQueryExecutionError>();
            var result = new List<PageData>();

            var expressionParser = new CmsqlExpressionParser();
            foreach (CmsqlQuery query in queries)
            {
                var contentType = _contentTypeRepository.Load(query.ContentType);
                if (contentType == null)
                {
                    errors.Add(new CmsqlQueryExecutionError($"Couldn't load content-type '{query.ContentType}'."));
                    continue;
                }

                var visitorContext = expressionParser.Parse(contentType, query.Criteria);
                if (visitorContext.Errors.Any())
                {
                    errors.AddRange(visitorContext.Errors);
                    continue;
                }

                var searchStartNodeRef = GetStartSearchFromNode(query.StartNode);
                if (PageReference.IsNullOrEmpty(searchStartNodeRef))
                {
                    errors.Add(new CmsqlQueryExecutionError($"Couldn't process start node '{query.StartNode}'."));
                    continue;
                }

                foreach (var propertyCriteriaCollection in visitorContext.GetCriteria())
                {
                    var foundPages = _pageCriteriaQueryService.FindPagesWithCriteria(
                        searchStartNodeRef,
                        propertyCriteriaCollection);
                    if (foundPages != null && foundPages.Any())
                    {
                        result.AddRange(foundPages);
                    }
                }
            }

            var pageDataCmsqlQueryResults =
                result.Select(p => new PageDataCmsqlQueryResult(p)).ToList();

            return new CmsqlQueryExecutionResult(pageDataCmsqlQueryResults, errors);
        }

        private static PageReference GetStartSearchFromNode(CmsqlQueryStartNode startNode)
        {
            switch (startNode.StartNodeType)
            {
                case CmsqlQueryStartNodeType.Start:
                    return ContentReference.StartPage;
                case CmsqlQueryStartNodeType.Root:
                    return ContentReference.RootPage;
                case CmsqlQueryStartNodeType.Id:
                    if (int.TryParse(startNode.StartNodeId, out int rootNodeId))
                    {
                        return new PageReference(rootNodeId);
                    }
                    break;
            }

            return PageReference.EmptyReference;
        }
    }
}
