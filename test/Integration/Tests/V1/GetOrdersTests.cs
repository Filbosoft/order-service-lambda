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
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System.Linq;
using Conditus.DynamoDB.QueryExtensions.Extensions;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;
using static Integration.Seeds.V1.PortfolioSeeds;
using static Integration.Seeds.V1.AssetSeeds;

namespace Integration.Tests.V1
{
    public class GetOrdersTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _db;

        public GetOrdersTests(CustomWebApplicationFactory<Startup> factory)
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
                ACTIVE_BUY_ORDER,
                COMPLETED_BUY_ORDER,
                ACTIVE_SELL_ORDER,
                COMPLETED_SELL_ORDER,
                OLD_ORDER,
                TEN_YEAR_OLD_ORDER,
                COMPLETED_NONUSER_ORDER,
                COMPLETED_ORDER_FROM_ANOTHER_PORTFOLIO,
                COMPLETED_NONEXISTING_ASSET_ORDER,
                ORDER_COMPLETED_TODAY
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
        public async void GetOrders_WithoutQueryParameters_ShouldReturnUserOrders10YearsBack()
        {
            //Given
            //Orders seeded

            //When
            var httpResponse = await _client.GetAsync(BASE_URL);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedAt >= DateTime.UtcNow.AddYears(-10))
                .And.NotContain(o => o.Id.Equals(OLD_ORDER.Id));
        }

        [Fact]
        public async void GetOrders_WithPortfolioId_ShouldReturnAllPortfolioOrders()
        {
            //Given
            var query = $"portfolioId={USER_DKK_PORTFOLIO.Id}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.NotContain(o => o.Id.Equals(COMPLETED_ORDER_FROM_ANOTHER_PORTFOLIO))
                .And.OnlyContain(o => o.PortfolioName.Equals(USER_DKK_PORTFOLIO.Name));
        }

        [Fact]
        public async void GetOrders_WithPortfolioIdWhichIsNotTestUsers_ShouldReturnBadRequest()
        {
            //Given
            var query = $"portfolioId={NONTESTUSER_PORTFOLIO}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Equals(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void GetOrders_WithAssetSymbol_ShouldReturnUserOrdersFilteredByAssetId()
        {
            //Given
            var assetSymbol = ORDER_COMPLETED_TODAY.AssetSymbol;
            var query = $"assetSymbol={DKK_STOCK.Symbol}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.AssetSymbol.Equals(DKK_STOCK.Symbol));
        }

        [Fact]
        public async void GetOrders_WithType_ShouldReturnUserOrdersFilteredByType()
        {
            //Given
            var query = $"type={OrderType.Buy}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.Type.Equals(OrderType.Buy));
        }

        [Fact]
        public async void GetOrders_WithStatus_ShouldReturnUserOrdersFilteredByStatus()
        {
            //Given
            var query = $"status={OrderStatus.Completed}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.Status.Equals(OrderStatus.Completed));
        }

        [Fact]
        public async void GetOrders_WithCreatedFromDate_ShouldReturnUserOrdersCreatedAfterThePassedDate()
        {
            //Given
            var query = $"createdFromDate={COMPLETED_BUY_ORDER.CreatedAt}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedAt >= COMPLETED_BUY_ORDER.CreatedAt);
        }

        [Fact]
        public async void GetOrders_WithCreatedToDate_ShouldReturnUserOrdersCreatedBeforeThePassedDate()
        {
            //Given
            var createdToDate = COMPLETED_BUY_ORDER.CreatedAt;
            var query = $"createdToDate={createdToDate}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedAt <= createdToDate);
        }

        [Fact]
        public async void GetOrders_WithCompletedFromDate_ShouldReturnUserOrdersCompletedAfterThePassedDate()
        {
            //Given
            var query = $"completedFromDate={COMPLETED_BUY_ORDER.CompletedAt}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CompletedAt >= COMPLETED_BUY_ORDER.CompletedAt);
        }

        [Fact]
        public async void GetOrders_WithCompletedToDate_ShouldReturnUserOrdersCompletedBeforeThePassedDate()
        {
            //Given
            var completedToDate = COMPLETED_BUY_ORDER.CompletedAt;
            var query = $"completedToDate={completedToDate}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<PagedApiResponse<IEnumerable<OrderOverview>>>();
            var orders = apiResponse.Data;

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CompletedAt <= completedToDate);
        }
    }
}
