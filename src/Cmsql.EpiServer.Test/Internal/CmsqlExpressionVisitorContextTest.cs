using System.Collections.Generic;
using System.Linq;
using Cmsql.Optimizely.Internal;
using EPiServer;
using EPiServer.DataAbstraction;
using FluentAssertions;
using Xunit;

namespace Cmsql.EpiServer.Test.Internal
{
    public class CmsqlExpressionVisitorContextTest
    {
        [Fact]
        public void Test_can_add_criteria_when_no_criteria_collection_has_been_pushed_yet()
        {
            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());

            context.AddPropertyCriteria(new PropertyCriteria());

            IEnumerable<PropertyCriteriaCollection> criteria = context.GetCriteria().ToList();
            criteria.Should().HaveCount(1);
            criteria.Single().Should().HaveCount(2);
        }

        [Fact]
        public void Test_can_add_criteria_when_criteria_collection_has_been_pushed()
        {
            CmsqlExpressionVisitorContext context = new CmsqlExpressionVisitorContext(new ContentType());
            
            context.AddPropertyCriteria(new PropertyCriteria());

            IEnumerable<PropertyCriteriaCollection> criteria = context.GetCriteria().ToList();
            criteria.Should().HaveCount(1);
            criteria.Single().Should().HaveCount(2);
        }
    }
}
