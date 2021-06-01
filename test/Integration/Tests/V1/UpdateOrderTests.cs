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
using Business.Commands;
using Conditus.Trader.Domain.Entities;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;

namespace Integration.Tests.V1
{
    public class UpdateOrderTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;

        public UpdateOrderTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateAuthorizedClient();
            _dbContext = factory.GetDynamoDBContext();

            Setup();
        }

        public void Dispose()
        {
            _client.Dispose();
            _dbContext.Dispose();
        }

        public async void Setup()
        {
            var seedOrders = new List<OrderEntity>
            {
                COMPLETED_BUY_ORDER,
                EXPIRED_BUY_ORDER,
                CANCELLED_BUY_ORDER
            };

            var batchWrite = _dbContext.CreateBatchWrite<OrderEntity>();
            batchWrite.AddPutItems(seedOrders);

            await batchWrite.ExecuteAsync();
        }

        [Theory]
        [InlineData(EXPIRES_BUY_ORDER_ID)]
        [InlineData(COMPLETED_BUY_ORDER_ID)]
        [InlineData(CANCELLED_BUY_ORDER_ID)]
        public async void UpdateOrder_WhereOrderIsNotActive_ShouldReturnBadRequest(string orderId)
        {
            //Given
            var uri = $"{BASE_URL}/{orderId}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 100.1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void UpdateOrder_WithValidPrice_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            await _dbContext.SaveAsync<OrderEntity>(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 0.1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var updatedOrder = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .ExcludingMissingMembers());
            updatedOrder.Price.Should().Be(orderUpdater.Price);

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, updatedOrder.CreatedAt);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.Price)
                    .ExcludingMissingMembers());
            dbOrder.Price.Should().Be(orderUpdater.Price);
        }

        [Fact]
        public async void UpdateOrder_WithExpiresAt_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            await _dbContext.SaveAsync<OrderEntity>(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                ExpiresAt = DateTime.UtcNow.AddDays(5)
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var updatedOrder = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.ExpiresAt)
                    .ExcludingMissingMembers());
            updatedOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, updatedOrder.CreatedAt);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.ExpiresAt)
                    .ExcludingMissingMembers());
            dbOrder.ExpiresAt.Should().BeCloseTo((DateTime)orderUpdater.ExpiresAt, 60000);
        }

        [Fact]
        public async void UpdateOrder_WithCancel_ShouldReturnAcceptedAndTheUpdatedOrder()
        {
            //Given
            await _dbContext.SaveAsync<OrderEntity>(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Cancel = true
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            var updatedOrder = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            updatedOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.OrderStatus)
                    .ExcludingMissingMembers());
            updatedOrder.Status.Should().Be(OrderStatus.Cancelled);

            var dbOrder = await _dbContext.LoadAsync<OrderEntity>(TESTUSER_ID, updatedOrder.CreatedAt);

            dbOrder.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, options => options
                    .Excluding(o => o.OrderStatus)
                    .ExcludingMissingMembers());
            dbOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public async void UpdateOrder_WithNonUserOrderId_ShouldReturnNotFound()
        {
            //Given
            await _dbContext.SaveAsync<OrderEntity>(ACTIVE_NONUSER_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_NONUSER_ORDER.Id}";
            var orderUpdater = new UpdateOrderCommand
            {
                Price = 1M
            };

            //When
            var httpResponse = await _client.PutAsync(uri, HttpSerializer.GetStringContent(orderUpdater));

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
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