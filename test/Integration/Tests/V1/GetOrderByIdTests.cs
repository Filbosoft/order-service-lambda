using System;
using System.Collections.Generic;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Conditus.Trader.Domain.Entities;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;

namespace Integration.Tests.V1
{
    public class GetOrderByIdTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IDynamoDBContext _dbContext;
        public CustomWebApplicationFactory<Startup> _factory;

        public GetOrderByIdTests(CustomWebApplicationFactory<Startup> factory)
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
                ACTIVE_BUY_ORDER
            };

            var batchWrite = _dbContext.CreateBatchWrite<OrderEntity>();
            batchWrite.AddPutItems(seedOrders);

            await batchWrite.ExecuteAsync();
        }

        [Fact]
        public async void GetOrderById_WithValidId_ShouldReturnSeededOrder()
        {
            //Given
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var order = await httpResponse.GetDeserializedResponseBodyAsync<OrderDetail>();

            order.Should().NotBeNull()
                .And.BeEquivalentTo(ACTIVE_BUY_ORDER, o =>
                    o.ExcludingMissingMembers());
        }

        [Fact]
        public async void GetOrderById_WithInvalidId_ShouldReturnNotFound()
        {
            //Given
            var uri = $"{BASE_URL}/{Guid.NewGuid()}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async void GetOrderById_WithOrderNotBelongingToUser_ShouldReturnNotFound()
        {
            //Given
            var uri = $"{BASE_URL}/{COMPLETED_NONUSER_ORDER.Id}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
