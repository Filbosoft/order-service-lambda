using System;
using System.Collections.Generic;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Business.Commands;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;

namespace Integration.Tests.V1
{
    public class UpdateOrderTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IAmazonDynamoDB _db;

        public UpdateOrderTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
            _db = factory.GetDynamoDB();
        }

        public void Dispose()
        {
            _client.Dispose();
            _db.Dispose();
        }

        public async void SeedOrder(OrderEntity seedOrder)
        {
            await _db.PutItemAsync(typeof(OrderEntity).GetDynamoDBTableName(), seedOrder.GetAttributeValueMap());
        }

        [Theory]
        [MemberData(nameof(NonActiveOrders))]
        public async void UpdateOrder_WhereOrderIsNotActive_ShouldReturnBadRequest(OrderEntity nonActiveOrder)
        {
            //Given
            SeedOrder(nonActiveOrder);
            var uri = $"{BASE_URL}/{nonActiveOrder.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 100.1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(UpdateOrderResponseCodes.OrderNotActive.ToString());
        }

        public static IEnumerable<object[]> NonActiveOrders 
        {
            get
            {
                yield return new object[] {EXPIRED_BUY_ORDER};
                yield return new object[] {COMPLETED_BUY_ORDER};
                yield return new object[] {CANCELLED_BUY_ORDER};
            }
        }

        [Fact]
        public async void UpdateOrder_WithValidPrice_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            SeedOrder(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 0.1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var updatedOrder = apiResponse.Data;

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .ExcludingMissingMembers());
            updatedOrder.Price.Should().Be(orderUpdater.Price);

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(),
                updatedOrder.Id.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .Excluding(o => o.OrderStatusCreatedAtCompositeKey)
                    .ExcludingMissingMembers());
            dbOrder.Price.Should().Be(orderUpdater.Price);
        }

        [Fact]
        public async void UpdateOrder_WithPriceAndExpiresAt_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            SeedOrder(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 0.1M,
                ExpiresAt = DateTime.UtcNow.AddDays(5)
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var updatedOrder = apiResponse.Data;

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .Excluding(o => o.ExpiresAt)
                    .ExcludingMissingMembers());
            updatedOrder.Price.Should().Be(orderUpdater.Price);
            updatedOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(),
                updatedOrder.Id.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .Excluding(o => o.ExpiresAt)
                    .Excluding(o => o.OrderStatusCreatedAtCompositeKey)
                    .ExcludingMissingMembers());
            dbOrder.Price.Should().Be(orderUpdater.Price);
            dbOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);
        }

        [Fact]
        public async void UpdateOrder_WithExpiresAt_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            SeedOrder(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                ExpiresAt = DateTime.UtcNow.AddDays(5)
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var updatedOrder = apiResponse.Data;

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.ExpiresAt)
                    .ExcludingMissingMembers());
            updatedOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(),
                updatedOrder.Id.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.ExpiresAt)
                    .Excluding(o => o.OrderStatusCreatedAtCompositeKey)
                    .ExcludingMissingMembers());
            dbOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);
        }

        [Fact]
        public async void UpdateOrder_WithCancel_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            SeedOrder(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Cancel = true
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var updatedOrder = apiResponse.Data;

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.OrderStatus)
                    .ExcludingMissingMembers());
            updatedOrder.Status.Should().Be(OrderStatus.Cancelled);

            var dbOrder = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                TESTUSER_ID.GetAttributeValue(),
                updatedOrder.Id.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.OrderStatus)
                    .Excluding(o => o.OrderStatusCreatedAtCompositeKey)
                    .ExcludingMissingMembers());
            dbOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);
            dbOrder.OrderStatusCreatedAtCompositeKey.Should().StartWith(((int) OrderStatus.Cancelled).ToString());
        }

        [Fact]
        public async void UpdateOrder_WithNonUserOrderId_ShouldReturnNotFound()
        {
            //Given
            SeedOrder(ACTIVE_NONUSER_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_NONUSER_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(UpdateOrderResponseCodes.OrderNotFound.ToString());
        }

        [Fact]
        public async void UpdateOrder_WithNonExistingOrderId_ShouldReturnNotFound()
        {
            //Given
            var uri = $"{BASE_URL}/{Guid.NewGuid()}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            var problem = await httpResponse.GetDeserializedResponseBodyAsync<ProblemDetails>();

            problem.Title.Should().Be(UpdateOrderResponseCodes.OrderNotFound.ToString());
        }

        [Fact]
        public async void UpdateOrder_WithNegativePrice_ShouldReturnBadRequest()
        {
            //Given
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = -1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void UpdateOrder_WithNegativeQuantity_ShouldReturnBadRequest()
        {
            //Given
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Quantity = -1
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }
}