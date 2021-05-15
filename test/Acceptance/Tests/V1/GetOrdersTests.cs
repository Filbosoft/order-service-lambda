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

namespace Acceptance.Tests.V1
{
    public class GetOrdersTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        private const string BASE_URL = "api/v1";
        public CustomWebApplicationFactory<Startup> _factory;
        
        

        public GetOrdersTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _dbContext = factory.GetDynamoDBContext();

            Seed();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        /***
        * Seed values
        ***/

        private const string User1Id = "c525999f-840b-48b3-8b42-fe466ada9a45";
        private const string User1Portfolio1Id = "a8f0914b-13c4-4de2-bd6b-c85f978ab9d3";
        private const string User1Portfolio2Id = "565eabea-91b0-495f-848a-98e6979d63f2";
        private const string User2Id = "32971179-33fd-496a-81a6-9eb9a90cc59c";
        private const string User2Portfolio1Id = "95e54e8e-606a-40cd-b29e-01b2aff646dc";
        private const string User2Portfolio2Id = "8aa715fb-804e-4d6e-9d60-7f2b4b2af4a7";


        private const string Stock1Id = "4af4d567-2f92-425a-89ee-dfc12d77aba8";
        private const string Stock2Id = "67dbae27-2ad7-48ef-ba13-b6df7bb25cb8";

        private const string Valuta1Id = "74dbdfb4-81e8-49f4-920b-8b94b953aada";
        private const string Valuta2Id = "9b1b1f96-aa22-4499-b21c-281302fa46d8";

        private readonly Order Order1 = new Order
        {
            Id = "e365a51c-b176-494f-8506-1c80cb84a69b",
            CreatedBy = User1Id,
            PortfolioId = User1Portfolio1Id,
            AssetId = Stock1Id,
            AssetType = AssetType.Stock,
            Type = OrderType.Buy,
            Status = OrderStatus.Completed,
            Currency = "DKK",
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:00 AM"),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM")
        };

        private async void Seed()
        {
            var seedOrders = new List<Order>
            {
                Order1
            };

            var batchWrite = _dbContext.CreateBatchWrite<Order>();
            batchWrite.AddPutItems(seedOrders);

            await batchWrite.ExecuteAsync();
        }

        [Fact]
        public async void GetOrders_WithoutQueryParameters_ShouldReturnAllUserOrders()
        {
            var token = await _factory.GetTestUserToken();
            //Given
            //Orders seeded

            //When
            var httpResponse = await _client.GetAsync(BASE_URL);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithPortfolioId_ShouldReturnAllPortfolioOrders()
        {
            //Given
            var query = $"portfolioId={User1Portfolio1Id}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.PortfolioId.Equals(User1Portfolio1Id))
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithPortfolioIdWhichIsNotUsers_ShouldReturnUnauthorized()
        {
            //Given
            var query = $"portfolioId={User2Portfolio1Id}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Equals(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async void GetOrders_WithAssetId_ShouldReturnUserOrdersFilteredByAssetId()
        {
            //Given
            var query = $"assetId={Stock1Id}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.AssetId.Equals(Stock1Id))
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.Type.Equals(OrderType.Buy))
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
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
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.Status.Equals(OrderStatus.Completed))
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithCreatedFromDate_ShouldReturnUserOrdersCreatedAfterThePassedDate()
        {
            //Given
            var query = $"createdFromDate={Order1.CreatedAt}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedAt >= Order1.CreatedAt)
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithCreatedToDate_ShouldReturnUserOrdersCreatedBeforeThePassedDate()
        {
            //Given
            var createdToDate = Order1.CreatedAt.AddDays(5);
            var query = $"createdToDate={createdToDate}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CreatedAt <= createdToDate)
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithCompletedFromDate_ShouldReturnUserOrdersCompletedAfterThePassedDate()
        {
            //Given
            var query = $"completedFromDate={Order1.CompletedAt}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CompletedAt >= Order1.CompletedAt)
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }

        [Fact]
        public async void GetOrders_WithCompletedToDate_ShouldReturnUserOrdersCompletedBeforeThePassedDate()
        {
            //Given
            var completedToDate = Order1.CompletedAt?.AddDays(5);
            var query = $"completedToDate={completedToDate}";
            var uri = $"{BASE_URL}?{query}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var orders = await httpResponse.GetDeserializedResponseBodyAsync<IEnumerable<Order>>();

            orders.Should().NotBeNullOrEmpty()
                .And.OnlyContain(o => o.CompletedAt <= completedToDate)
                .And.OnlyContain(o => o.CreatedBy.Equals(User1Id));
        }
    }
}
