using System;
using System.Collections.Generic;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;
using Conditus.DynamoDB.QueryExtensions.Pagination;
using Amazon.DynamoDBv2.Model;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Amazon.DynamoDBv2;
using System.Linq;
using Conditus.DynamoDB.QueryExtensions.Extensions;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;
using static Integration.Seeds.V1.PortfolioSeeds;
using static Integration.Seeds.V1.AssetSeeds;

namespace Integration.Tests.V1
{
    public class GetOrdersPaginationTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _db;

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
                PAGINATION_BUY_ORDER1,
                PAGINATION_SELL_ORDER1,
                PAGINATION_BUY_ORDER2,
                PAGINATION_SELL_ORDER2,
                PAGINATION_BUY_ORDER3,
                PAGINATION_SELL_ORDER3
            };

            var writeRequests = seedOrders
                .Select(o => new PutRequest{ Item = o.GetAttributeValueMap()})
                .Select(p => new WriteRequest{ PutRequest = p})
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
            var pageSize = 2;
            var query = $"pageSize={pageSize}"
                 + $"&portfolioId={PAGINATION_BUY_ORDER1.PortfolioId}&type={PAGINATION_BUY_ORDER1.OrderType}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.HaveCount(pageSize);

            apiResponse.Pagination.Should().NotBeNull();
            apiResponse.Pagination.PageSize.Should().Be(pageSize);
            apiResponse.Pagination.PaginationToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void GetOrders_WithPaginationToken_ShouldReturnUserOrdersPaginatedByTheProvidedToken()
        {
            //Given
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {nameof(OrderEntity.OwnerId), PAGINATION_BUY_ORDER1.OwnerId.GetAttributeValue()},
                {nameof(OrderEntity.CreatedAt), PAGINATION_BUY_ORDER1.CreatedAt.GetAttributeValue()},
                {nameof(OrderEntity.PortfolioId), SelfContainingCompositeKeyMapper.GetSelfContainingCompositeKeyAttributeValue(PAGINATION_BUY_ORDER1, nameof(PAGINATION_BUY_ORDER1.PortfolioId))}
            };
            var paginationToken = PaginationTokenConverter.GetToken<OrderEntity>(lastEvaluatedKey);
            var pageSize = 2;
            var query = $"pageSize={pageSize}&portfolioId={PAGINATION_BUY_ORDER1.PortfolioId}&type={PAGINATION_BUY_ORDER1.OrderType}&paginationToken={paginationToken}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.NotContain(o => o.Id.Equals(PAGINATION_BUY_ORDER1.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_BUY_ORDER2.Id))
                .And.Contain(o => o.Id.Equals(PAGINATION_BUY_ORDER3.Id));
            
        }
    }
}
