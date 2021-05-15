using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using Newtonsoft.Json;

using API;


namespace API.Tests
{
    public class GetOrdersTests
    {
        private readonly LambdaEntryPoint _entryPoint = new LambdaEntryPoint();
        public readonly TestLambdaContext _context = new TestLambdaContext();

        public GetOrdersTests()
        { }

        [Fact]
        public void GetOrders_WithoutQueryParameters_ShouldReturnAllUserOrders()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithPortfolioId_ShouldReturnAllPortfolioOrders()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithPortfolioIdWhichIsNotUsers_ShouldReturnUnauthorized()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithAssetId_ShouldReturnUserOrdersFilteredByAssetId()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithType_ShouldReturnUserOrdersFilteredByType()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithStatus_ShouldReturnUserOrdersFilteredByStatus()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithCurrency_ShouldReturnUserOrdersFilteredByCurrency()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithCreatedFromDate_ShouldReturnUserOrdersCreatedAfterThePassedDate()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithCreatedToDate_ShouldReturnUserOrdersCreatedBeforeThePassedDate()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithCompletedFromDate_ShouldReturnUserOrdersCompletedAfterThePassedDate()
        {
            //Given

            //When

            //Then
        }

        [Fact]
        public void GetOrders_WithCompletedToDate_ShouldReturnUserOrdersCompletedBeforeThePassedDate()
        {
            //Given

            //When

            //Then
        }
    }
}
