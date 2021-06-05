using System;
using Xunit;
using Api;
using System.Net.Http;
using Conditus.Trader.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Integration.Utilities;
using Conditus.Trader.Domain.Entities;
using Api.Responses.V1;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.DynamoDB.MappingExtensions.Mappers;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.OrderSeeds;

namespace Integration.Tests.V1
{
    public class GetOrderByIdTests : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IAmazonDynamoDB _db;

        public GetOrderByIdTests(CustomWebApplicationFactory<Startup> factory)
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

        [Fact]
        public async void GetOrderById_WithValidId_ShouldReturnSeededOrder()
        {
            //Given
            SeedOrder(ACTIVE_BUY_ORDER);
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
            SeedOrder(COMPLETED_NONUSER_ORDER);
            var uri = $"{BASE_URL}/{COMPLETED_NONUSER_ORDER.Id}";

            //When
            var httpResponse = await _client.GetAsync(uri);

            //Then
            httpResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
