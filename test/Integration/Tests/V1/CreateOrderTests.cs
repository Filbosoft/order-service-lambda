using System;
using System.Collections.Generic;
using System.Net;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Business.Commands;
using FluentAssertions.Execution;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.AssetSeeds;
using static Integration.Seeds.V1.PortfolioSeeds;
using static Integration.Seeds.V1.CurrencySeeds;

namespace Integration.Tests.V1
{
    public class CreateOrderTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IAmazonDynamoDB _db;

        public CreateOrderTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
            _db = factory.GetDynamoDB();
        }

        public void Dispose()
        {
            _client.Dispose();
            _db.Dispose();
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
                PortfolioId = USER_DKK_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var newOrder = apiResponse.Data;

            using (new AssertionScope())
            {
                newOrder.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers());

                newOrder.Id.Should().NotBeNullOrEmpty();
                newOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                newOrder.AssetType.Should().Be(DKK_STOCK.Type);
                newOrder.Status.Should().Be(OrderStatus.Active);
            }

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(), 
                newOrder.Id.GetAttributeValue(), 
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

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
            var price = USER_DKK_PORTFOLIO.Capital + 1;
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = DKK_STOCK.Symbol,
                Price = price,
                Quantity = 1,
                PortfolioId = USER_DKK_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(CreateOrderResponseCodes.ValidationFailed.ToString());
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
                PortfolioId = USER_DKK_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var newOrder = apiResponse.Data;

            using (new AssertionScope())
            {
                newOrder.Should().NotBeNull()
                    .And.BeEquivalentTo(createOrderCommand, o =>
                        o.ExcludingMissingMembers());

                newOrder.Id.Should().NotBeNullOrEmpty();
                newOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 60000);
                newOrder.AssetType.Should().Be(DKK_STOCK.Type);
                newOrder.Status.Should().Be(OrderStatus.Active);
            }

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(), 
                newOrder.Id.GetAttributeValue(), 
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

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
                PortfolioId = USER_DKK_PORTFOLIO.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(CreateOrderResponseCodes.ValidationFailed.ToString());
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
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(CreateOrderResponseCodes.PortfolioNotFound.ToString());
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
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(CreateOrderResponseCodes.PortfolioNotFound.ToString());
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
                PortfolioId = USER_DKK_PORTFOLIO.Id
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var newOrder = apiResponse.Data;

            newOrder.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), 60000);

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(), 
                newOrder.Id.GetAttributeValue(), 
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

            dbOrder.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), 60000);
        }

        [Fact]
        public async void CreateOrder_WithDifferentAssetCurrencyThanPortfolioCurrencyAndInsufficientCapital_ShouldReturnBadRequest()
        {
            //Given
            var price = (USER_DKK_PORTFOLIO.Capital / CONVERSION_RATE) + 1; 
            var createOrderCommand = new CreateOrderCommand
            {
                Type = OrderType.Buy,
                AssetSymbol = USD_STOCK.Symbol,
                Price = price,
                Quantity = 1,
                PortfolioId = USER_DKK_PORTFOLIO.Id
            };

            //When
            var httpResponse = await _client.PostAsync(BASE_URL, HttpSerializer.GetStringContent(createOrderCommand));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(CreateOrderResponseCodes.ValidationFailed.ToString());
        }
    }
}