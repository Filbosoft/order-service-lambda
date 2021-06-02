using System;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;

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
        }

        public void Dispose()
        {
            _client.Dispose();
            _dbContext.Dispose();
        }

        [Fact]
        public async void GetOrderById_WithValidId_ShouldReturnSeededOrder()
        {
            //Given
            await _dbContext.SaveAsync<OrderEntity>(ACTIVE_BUY_ORDER);
            var uri = $"{BASE_URL}/{ACTIVE_BUY_ORDER.Id}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.EnsureSuccessStatusCode();
            var apiResponse = await httpResponse.GetDeserializedResponseBodyAsync<ApiResponse<OrderDetail>>();
            var order = apiResponse.Data;

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
            await _dbContext.SaveAsync<OrderEntity>(COMPLETED_NONUSER_ORDER);
            var uri = $"{BASE_URL}/{COMPLETED_NONUSER_ORDER.Id}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
