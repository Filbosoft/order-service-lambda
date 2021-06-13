using System;
using System.Collections.Generic;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Integration.Utilities;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;
using Conditus.DynamoDB.QueryExtensions.Pagination;
using Amazon.DynamoDBv2.Model;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Amazon.DynamoDBv2;
using System.Linq;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Business.Queries;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;

namespace Integration.Tests.V1
{
    public class GetOrdersPaginationTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _db;
        private const string ORDER_QUERY_URL = BASE_URL + "/query";

        public GetOrdersPaginationTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
            _db = factory.GetDynamoDB();
            _dbContext = factory.GetDynamoDBContext();

            Seed();
        }

        public void Dispose()
        {
            _client.Dispose();
            _dbContext.Dispose();
        }

        private async void Seed()
        {
            var seedOrders = new List<OrderEntity>
            {
                PAGINATION_ACTIVE_BUY_ORDER1,
                PAGINATION_ACTIVE_SELL_ORDER1,
                PAGINATION_ACTIVE_BUY_ORDER2,
                PAGINATION_ACTIVE_SELL_ORDER2,
                PAGINATION_ACTIVE_BUY_ORDER3,
                PAGINATION_ACTIVE_SELL_ORDER3
            };

            var writeRequests = seedOrders
                .Select(o => new PutRequest { Item = o.GetAttributeValueMap() })
                .Select(p => new WriteRequest { PutRequest = p })
                .ToList();

            var batchWriteRequest = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    { typeof(OrderEntity).GetDynamoDBTableName(), writeRequests }
                }
            };

            await _db.BatchWriteItemAsync(batchWriteRequest);
        }

        [Fact]
        public async void GetOrders_WithPageSize_ShouldReturnSpecifiedPageSizeUserOrdersWithPagination()
        {
            //Given
            var query = new GetOrdersQuery
            {
                PageSize = 2,
                PortfolioId = PAGINATION_ACTIVE_BUY_ORDER3.PortfolioId,
                Type = PAGINATION_ACTIVE_BUY_ORDER3.OrderType,
                CreatedToDate = PAGINATION_ACTIVE_BUY_ORDER3.CreatedAt
            };

            //When
            var httpResponse = await _client.PostAsync(ORDER_QUERY_URL, HttpSerializer.GetStringContent(query));

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.HaveCount(query.PageSize);

            apiResponse.Pagination.Should().NotBeNull();
            apiResponse.Pagination.PageSize.Should().Be(query.PageSize);
            apiResponse.Pagination.PaginationToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void GetOrders_WithPaginationToken_ShouldReturnUserOrdersPaginatedByTheProvidedToken()
        {
            //Given
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {nameof(OrderEntity.OwnerId), PAGINATION_ACTIVE_BUY_ORDER3.OwnerId.GetAttributeValue()},
                {nameof(OrderEntity.CreatedAt), PAGINATION_ACTIVE_BUY_ORDER3.CreatedAt.GetAttributeValue()},
                {nameof(OrderEntity.PortfolioId), SelfContainingCompositeKeyMapper.GetSelfContainingCompositeKeyAttributeValue(PAGINATION_ACTIVE_BUY_ORDER3, nameof(PAGINATION_ACTIVE_BUY_ORDER3.PortfolioId))}
            };
            var paginationToken = PaginationTokenConverter.GetToken<OrderEntity>(lastEvaluatedKey);
            var query = new GetOrdersQuery
            {
                PortfolioId = PAGINATION_ACTIVE_BUY_ORDER3.PortfolioId,
                Type = PAGINATION_ACTIVE_BUY_ORDER3.OrderType,
                CreatedToDate = PAGINATION_ACTIVE_BUY_ORDER3.CreatedAt,
                PageSize = 2,
                PaginationToken = paginationToken
            };

            //When
            var httpResponse = await _client.PostAsync(ORDER_QUERY_URL, HttpSerializer.GetStringContent(query));

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.NotContain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER3.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER2.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER1.Id));

        }

        [Fact]
        public async void GetOrders_WithActiveStatusAndPaginationToken_ShouldReturnUserActiveOrdersPaginatedByTheProvidedToken()
        {
            //Given
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {nameof(OrderEntity.OwnerId), PAGINATION_ACTIVE_BUY_ORDER3.OwnerId.GetAttributeValue()},
                {nameof(OrderEntity.CreatedAt), PAGINATION_ACTIVE_BUY_ORDER3.CreatedAt.GetAttributeValue()},
                {nameof(OrderEntity.OrderStatusCreatedAtCompositeKey), CompositeKeyMapper.GetCompositeKeyAttributeValue(PAGINATION_ACTIVE_BUY_ORDER3, nameof(PAGINATION_ACTIVE_BUY_ORDER3.OrderStatusCreatedAtCompositeKey))}
            };
            var paginationToken = PaginationTokenConverter.GetToken<OrderEntity>(lastEvaluatedKey);
            var query = new GetOrdersQuery
            {
                Type = PAGINATION_ACTIVE_BUY_ORDER3.OrderType,
                CreatedToDate = PAGINATION_ACTIVE_BUY_ORDER3.CreatedAt,
                Status = PAGINATION_ACTIVE_BUY_ORDER3.OrderStatus,
                PageSize = 2,
                PaginationToken = paginationToken
            };

            //When
            var httpResponse = await _client.PostAsync(ORDER_QUERY_URL, HttpSerializer.GetStringContent(query));

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.NotContain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER3.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER2.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_ACTIVE_BUY_ORDER1.Id));

        }
    }
}
