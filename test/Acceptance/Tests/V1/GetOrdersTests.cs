using System;
using System.Collections.Generic;

using Xunit;

using API;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Acceptance.Utilities;

using Conditus.Trader.Domain.Entities;

using static Acceptance.Utilities.TestConstants;
using static Acceptance.Seeds.OrderSeeds;
using static Acceptance.Seeds.PortfolioSeeds;
using static Acceptance.Seeds.AssetSeeds;

namespace Acceptance.Tests.V1
{
    public class GetOrdersTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        public CustomWebApplicationFactory<Startup> _factory;

        public GetOrdersTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
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

            var batchWrite = _dbContext.CreateBatchWrite<OrderEntity>();
            batchWrite.AddPutItems(seedOrders);

            await batchWrite.ExecuteAsync();
        }

        [Fact]
        public async void GetOrders_WithoutQueryParameters_ShouldReturnUserOrders10YearsBack()
        {
            //Given
            //Orders seeded
            var dbOrders = await _dbContext.ScanAsync<OrderEntity>(new List<ScanCondition>()).GetRemainingAsync();

            //When
            var httpResponse = await _client.GetAsync(BASE_URL);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

            orders.Should().NotBeNullOrEmpty()
                .And.Contain(o => o.Id.Equals(TEN_YEAR_OLD_ORDER.Id))
                .And.NotContain(o => o.Id.Equals(OLD_ORDER.Id));
        }

        [Fact]
        public async void GetOrders_WithPortfolioId_ShouldReturnAllPortfolioOrders()
        {
            //Given
            var query = $"portfolioId={TESTUSER_PORTFOLIO}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

            orders.Should().NotBeNullOrEmpty()
                .And.NotContain(o => o.Id.Equals(COMPLETED_ORDER_FROM_ANOTHER_PORTFOLIO));
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
        public async void GetOrders_WithAssetId_ShouldReturnUserOrdersFilteredByAssetId()
        {
            //Given
            var query = $"assetSymbol={DKK_STOCK.Symbol}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<OrderOverview>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CompletedAt <= completedToDate);
        }
    }
}
