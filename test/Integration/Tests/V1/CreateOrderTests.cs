using System;
using System.Collections.Generic;
using System.Net;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Business.Commands;
using FluentAssertions.Execution;
using Conditus.Trader.Domain.Entities;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.AssetSeeds;
using static Integration.Seeds.V1.PortfolioSeeds;

namespace Integration.Tests.V1
{
    public class CreateOrderTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;

        public CreateOrderTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
            _dbContext = factory.GetDynamoDBContext();
        }

        public void Dispose()
        {
            _client.Dispose();
            _dbContext.Dispose();
        }

        [Fact]
        public async void CreateBuyOrder_WithValidValues_ShouldReturnCreatedAndTheNewOrder()
        {
            //Given
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = 1,
                PortfolioId = TESTUSER_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var order = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            using (new AssertionScope())
            {
                order.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers());

                order.Id.Should().NotBeNullOrEmpty();
                order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                order.AssetType.Should().Be(DKK_STOCK.Type);
                order.Status.Should().Be(OrderStatus.Active);
            }

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, order.CreatedAt);

            using (new AssertionScope())
            {
                dbOrder.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers()
                        .Excluding(o => o.ExpiresAt));

                dbOrder.Id.Should().NotBeNullOrEmpty();
                dbOrder.OwnerId.Should().Be(TESTUSER_ID);
                dbOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                dbOrder.ExpiresAt.Should().BeCloseTo((DateTime)createOrderCommand.ExpiresAt, 1000);
                dbOrder.AssetType.Should().Be(DKK_STOCK.Type);
                dbOrder.OrderStatus.Should().Be(OrderStatus.Active);
            }
        }

        [Fact]
        public async void CreateBuyOrder_WithInsufficientCapitalInPortfolio_ShouldReturnBadRequest()
        {
            //Given
            var price = TESTUSER_PORTFOLIO.Capital + 1;
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = price,
                Quantity = 1,
                PortfolioId = TESTUSER_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void CreateSellOrder_WithValidValues_ShouldReturnCreatedAndTheNewOrder()
        {
            //Given
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Sell,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = 100,
                PortfolioId = TESTUSER_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var order = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            using (new AssertionScope())
            {
                order.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers());

                order.Id.Should().NotBeNullOrEmpty();
                order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                order.AssetType.Should().Be(DKK_STOCK.Type);
                order.Status.Should().Be(OrderStatus.Active);
            }

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, order.CreatedAt);

            using (new AssertionScope())
            {
                dbOrder.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers()
                        .Excluding(o => o.ExpiresAt));

                dbOrder.Id.Should().NotBeNullOrEmpty();
                dbOrder.OwnerId.Should().Be(TESTUSER_ID);
                dbOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                dbOrder.ExpiresAt.Should().BeCloseTo((DateTime)createOrderCommand.ExpiresAt, 1000);
                dbOrder.AssetType.Should().Be(DKK_STOCK.Type);
                dbOrder.OrderStatus.Should().Be(OrderStatus.Active);
            }
        }

        [Fact]
        public async void CreateSellOrder_WithInsufficientAssetsInPortfolio_ShouldReturnBadRequest()
        {
            //Given
            var quantity = TESTUSERS_PORTFOLIO_STOCK_QUANTITY + 1;
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Sell,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = quantity,
                PortfolioId = TESTUSER_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void CreateOrder_WithInvalidPortfolioId_ShouldReturnBadRequest()
        {
            //Given
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = 1,
                PortfolioId = "7e97bdb6-91d0-443a-9f34-4aba7d11f039",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void CreateOrder_WithPortfolioIdNotBelongingToUser_ShouldReturnBadRequest()
        {
            //Given
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = 1,
                PortfolioId = NONTESTUSER_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Theory]
        [MemberData(nameof(CreateOrderCommandsWithMissingValues))]
        public async void CreateOrder_WithMissingValues_ShouldReturnBadRequest(CreateOrderCommand createOrderCommand)
        {
            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        public static IEnumerable<object[]> CreateOrderCommandsWithMissingValues
        {
            get
            {
                yield return new Object[] { new CreateOrderCommand { AssetSymbol = "Asset#1", Quantity = 10, Price = 150.5M, PortfolioId = "e8a3e754-b71c-4e56-afad-26a04dfbcc18", ExpiresAt = DateTime.UtcNow.AddMinutes(1) } };
                yield return new Object[] { new CreateOrderCommand { Type = OrderType.Buy, Quantity = 10, Price = 150.5M, PortfolioId = "e8a3e754-b71c-4e56-afad-26a04dfbcc18", ExpiresAt = DateTime.UtcNow.AddMinutes(1) } };
                yield return new Object[] { new CreateOrderCommand { Type = OrderType.Buy, AssetSymbol = "Asset#1", Price = 150.5M, PortfolioId = "e8a3e754-b71c-4e56-afad-26a04dfbcc18", ExpiresAt = DateTime.UtcNow.AddMinutes(1) } };
                yield return new Object[] { new CreateOrderCommand { Type = OrderType.Buy, AssetSymbol = "Asset#1", Quantity = 10, PortfolioId = "e8a3e754-b71c-4e56-afad-26a04dfbcc18", ExpiresAt = DateTime.UtcNow.AddMinutes(1) } };
                yield return new Object[] { new CreateOrderCommand { Type = OrderType.Buy, AssetSymbol = "Asset#1", Quantity = 10, Price = 150.5M, ExpiresAt = DateTime.UtcNow.AddMinutes(1) } };
                yield return new Object[] { new CreateOrderCommand { Type = OrderType.Buy, AssetSymbol = "Asset#1", Quantity = 10, Price = 150.5M, PortfolioId = "e8a3e754-b71c-4e56-afad-26a04dfbcc18" } };
            }
        }

        [Fact]
        public async void CreateOrder_WithoutExpirationDate_ShouldReturnCreatedWithExpirationSetToOneDayFromCreatedAt()
        {
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Sell,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = 100.1M,
                Quantity = 100,
                PortfolioId = TESTUSER_PORTFOLIO.Id
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var order = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            order.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), 60000);

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, order.CreatedAt);

            dbOrder.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), 60000);
        }
    }
}