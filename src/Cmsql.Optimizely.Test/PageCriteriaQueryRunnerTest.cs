using System.Linq;
using Cmsql.Query;
using Cmsql.Query.Execution;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cmsql.Optimizely.Test
{
    public class PageCriteriaQueryRunnerTest
    {
        private readonly Mock<IPageCriteriaQueryService> _pageCriteriaQueryServiceMock;
        private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
        private readonly PageCriteriaQueryRunner _runner;

        public PageCriteriaQueryRunnerTest()
        {
            _pageCriteriaQueryServiceMock = new Mock<IPageCriteriaQueryService>();
            _contentTypeRepositoryMock = new Mock<IContentTypeRepository>();
            _runner = new PageCriteriaQueryRunner(
                _pageCriteriaQueryServiceMock.Object,
                _contentTypeRepositoryMock.Object);
        }

        [Fact]
        public void Test_when_content_type_cannot_be_loaded_result_should_contain_error()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns((ContentType)null!);

            var query = new CmsqlQuery
            {
                ContentType = "NonExistentPage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = null
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Contain("NonExistentPage");
        }

        [Fact]
        public void Test_when_start_node_id_is_invalid_result_should_contain_error()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode
                {
                    StartNodeType = CmsqlQueryStartNodeType.Id,
                    StartNodeId = "not-a-number"
                },
                Criteria = null
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().StartWith("Couldn't process start node");
        }

        [Fact]
        public void Test_when_query_criteria_references_unknown_property_result_should_contain_error()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = new CmsqlQueryCondition
                {
                    Identifier = "UnknownProperty",
                    Operator = EqualityOperator.Equals,
                    Value = "test"
                }
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Contain("UnknownProperty");
        }

        [Fact]
        public void Test_when_valid_query_with_null_criteria_returns_pages()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            var page = new PageData();
            _pageCriteriaQueryServiceMock
                .Setup(s => s.FindPagesWithCriteria(
                    It.IsAny<PageReference>(),
                    It.IsAny<PropertyCriteriaCollection>(),
                    It.IsAny<string>(),
                    It.IsAny<ILanguageSelector>()))
                .Returns(new PageDataCollection { page });

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = null
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().BeEmpty();
            var pages = result.QueryResults.OfType<PageDataCmsqlQueryResult>().ToList();
            pages.Should().HaveCount(1);
            pages.Single().Page.Should().Be(page);
        }

        [Fact]
        public void Test_when_no_pages_are_found_result_should_have_no_query_results()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            _pageCriteriaQueryServiceMock
                .Setup(s => s.FindPagesWithCriteria(
                    It.IsAny<PageReference>(),
                    It.IsAny<PropertyCriteriaCollection>(),
                    It.IsAny<string>(),
                    It.IsAny<ILanguageSelector>()))
                .Returns((PageDataCollection)null!);

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = null
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().BeEmpty();
            result.QueryResults.Should().BeEmpty();
        }

        [Fact]
        public void Test_when_or_expression_find_pages_is_called_once_per_branch()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            var page1 = new PageData();
            var page2 = new PageData();
            _pageCriteriaQueryServiceMock
                .SetupSequence(s => s.FindPagesWithCriteria(
                    It.IsAny<PageReference>(),
                    It.IsAny<PropertyCriteriaCollection>(),
                    It.IsAny<string>(),
                    It.IsAny<ILanguageSelector>()))
                .Returns(new PageDataCollection { page1 })
                .Returns(new PageDataCollection { page2 });

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = new CmsqlQueryBinaryExpression
                {
                    Operator = ConditionalOperator.Or,
                    LeftExpression = new CmsqlQueryCondition
                    {
                        Identifier = MetaDataProperties.PageName,
                        Operator = EqualityOperator.Equals,
                        Value = "Page1"
                    },
                    RightExpression = new CmsqlQueryCondition
                    {
                        Identifier = MetaDataProperties.PageName,
                        Operator = EqualityOperator.Equals,
                        Value = "Page2"
                    }
                }
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().BeEmpty();
            _pageCriteriaQueryServiceMock.Verify(
                s => s.FindPagesWithCriteria(
                    It.IsAny<PageReference>(),
                    It.IsAny<PropertyCriteriaCollection>(),
                    It.IsAny<string>(),
                    It.IsAny<ILanguageSelector>()),
                Times.Exactly(2));
        }

        [Fact]
        public void Test_when_same_page_appears_in_multiple_or_branches_it_is_returned_once()
        {
            // Arrange
            _contentTypeRepositoryMock
                .Setup(r => r.Load(It.IsAny<string>()))
                .Returns(new ContentType { ID = 1 });

            // Both OR branches return the same page (same PageLink.ID).
            var page = new PageData();
            _pageCriteriaQueryServiceMock
                .Setup(s => s.FindPagesWithCriteria(
                    It.IsAny<PageReference>(),
                    It.IsAny<PropertyCriteriaCollection>(),
                    It.IsAny<string>(),
                    It.IsAny<ILanguageSelector>()))
                .Returns(new PageDataCollection { page });

            var query = new CmsqlQuery
            {
                ContentType = "SomePage",
                StartNode = new CmsqlQueryStartNode { StartNodeType = CmsqlQueryStartNodeType.Id, StartNodeId = "1" },
                Criteria = new CmsqlQueryBinaryExpression
                {
                    Operator = ConditionalOperator.Or,
                    LeftExpression = new CmsqlQueryCondition
                    {
                        Identifier = MetaDataProperties.PageName,
                        Operator = EqualityOperator.Equals,
                        Value = "Page1"
                    },
                    RightExpression = new CmsqlQueryCondition
                    {
                        Identifier = MetaDataProperties.PageName,
                        Operator = EqualityOperator.Equals,
                        Value = "Page2"
                    }
                }
            };

            // Act
            var result = _runner.ExecuteQueries(new[] { query });

            // Assert
            result.Errors.Should().BeEmpty();
            result.QueryResults.Should().HaveCount(1);
        }
    }
}
