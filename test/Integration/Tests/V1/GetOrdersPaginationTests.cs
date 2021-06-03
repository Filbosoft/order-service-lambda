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

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;
using static Integration.Seeds.V1.PortfolioSeeds;
using static Integration.Seeds.V1.AssetSeeds;
using Conditus.DynamoDB.MappingExtensions.Mappers;

namespace Integration.Tests.V1
{
    public class GetOrdersPaginationTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;

        public GetOrdersPaginationTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
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

            var batchWrite = _dbContext.CreateBatchWrite<OrderEntity>();
            batchWrite.AddPutItems(seedOrders);

            await batchWrite.ExecuteAsync();
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
            var orderAttributeValueMap = PAGINATION_BUY_ORDER1.GetAttributeValueMap();
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {nameof(OrderEntity.OwnerId), PAGINATION_BUY_ORDER1.OwnerId.GetAttributeValue()},
                {nameof(OrderEntity.CreatedAt), PAGINATION_BUY_ORDER1.CreatedAt.GetAttributeValue()},
                {nameof(OrderEntity.PortfolioId), PAGINATION_BUY_ORDER1.PortfolioId.GetAttributeValue()}
            };
            var paginationToken = PaginationTokenConverter.GetToken<OrderEntity>(lastEvaluatedKey);
            var pageSize = 2;
            var query = $"pageSize={pageSize}&paginationToken={paginationToken}"
                + $"&portfolioId={PAGINATION_BUY_ORDER1.PortfolioId}&type={PAGINATION_BUY_ORDER1.OrderType}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty();

            apiResponse.Pagination.Should().NotBeNull();
            apiResponse.Pagination.PaginationToken.Should().NotBeNullOrEmpty();
        }
    }
}
