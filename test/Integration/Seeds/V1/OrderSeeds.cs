using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;

using static Integration.Tests.V1.TestConstants;
using static Integration.Seeds.V1.PortfolioSeeds;
using static Integration.Seeds.V1.AssetSeeds;

namespace Integration.Seeds.V1
{
    public static class OrderSeeds
    {
        public const string COMPLETED_BUY_ORDER_ID = "e365a51c-b176-494f-8506-1c80cb84a69b";
        public static readonly OrderEntity COMPLETED_BUY_ORDER = new OrderEntity
        {
            Id = COMPLETED_BUY_ORDER_ID,
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:01 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:01 AM").ToUniversalTime()
        };

        public static readonly OrderEntity ACTIVE_BUY_ORDER = new OrderEntity
        {
            Id = "a3afc58f-1e54-43f0-9dd2-952ddf1a1130",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:02 AM").ToUniversalTime()
        };

        public static readonly OrderEntity COMPLETED_SELL_ORDER = new OrderEntity
        {
            Id = "bdb60a21-9639-4628-b53a-0de7651273bf",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Sell,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:03 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:03 AM").ToUniversalTime()
        };

        public static readonly OrderEntity ACTIVE_SELL_ORDER = new OrderEntity
        {
            Id = "566a3f10-a38f-4cf7-8b98-cfeecd67adfe",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Sell,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:04 AM").ToUniversalTime()
        };

        public static readonly OrderEntity OLD_ORDER = new OrderEntity
        {
            Id = "732c1796-d5c8-4a33-b680-67cc722457d7",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2000 08:00:05 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("2/5/2000 08:01:00 AM").ToUniversalTime()
        };
        public static readonly OrderEntity TEN_YEAR_OLD_ORDER = new OrderEntity
        {
            Id = "41c711a7-602a-406f-a070-12724f918060",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = DateTime.UtcNow.AddYears(-10).AddDays(1),
            CompletedAt = DateTime.UtcNow.AddYears(-10).AddDays(1).AddMinutes(1)
        };

        public static readonly OrderEntity COMPLETED_NONUSER_ORDER = new OrderEntity
        {
            Id = "48f3590d-7e0a-4414-ad04-8758e4b64260",
            OwnerId = "2a75d2bb-4f2b-4d8e-8167-a74c10f9b08d",
            PortfolioId = NONUSER_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:06 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity ACTIVE_NONUSER_ORDER = new OrderEntity
        {
            Id = "5f72672e-0388-4628-aadc-7d6fe90019de",
            OwnerId = "2a75d2bb-4f2b-4d8e-8167-a74c10f9b08d",
            PortfolioId = NONUSER_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/28/2021 08:00:07 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("6/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity COMPLETED_ORDER_FROM_ANOTHER_PORTFOLIO = new OrderEntity
        {
            Id = "623d62a5-e1cd-4444-bec0-514ed95fa6bc",
            OwnerId = TESTUSER_ID,
            PortfolioId = "1c440e1d-fbfd-4fcb-b72e-39bb05f5fdfa", //Doesn't exist
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:08 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity COMPLETED_NONEXISTING_ASSET_ORDER = new OrderEntity
        {
            Id = "2eb225a6-035c-462a-a781-ce8d8cf2d08c",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = "NonExistingAsset",
            AssetType = AssetType.Stock,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:09 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity ORDER_COMPLETED_TODAY = new OrderEntity
        {
            Id = "ce6ac2d2-f42a-4df0-8953-ae2be90f8c8d",
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = AssetType.Stock,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow.AddMinutes(-1)
        };

        public const string EXPIRES_BUY_ORDER_ID = "479cac8a-5ec0-44e1-b31f-21189be9c78c";
        public static readonly OrderEntity EXPIRED_BUY_ORDER = new OrderEntity
        {
            Id = EXPIRES_BUY_ORDER_ID,
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Expired,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:10 AM").ToUniversalTime(),
            ExpiresAt = Convert.ToDateTime("2/6/2021 08:00:00 AM").ToUniversalTime()
        };

        public const string CANCELLED_BUY_ORDER_ID = "f530f947-4814-41e9-9526-c97f48ff4e22";
        public static readonly OrderEntity CANCELLED_BUY_ORDER = new OrderEntity
        {
            Id = CANCELLED_BUY_ORDER_ID,
            OwnerId = TESTUSER_ID,
            PortfolioId = USER_DKK_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Cancelled,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:11 AM").ToUniversalTime(),
            ExpiresAt = Convert.ToDateTime("2/6/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity PAGINATION_ACTIVE_BUY_ORDER1 = new OrderEntity
        {
            Id = "42cc2dbe-955f-4cb0-92da-0aa5e59d4cee",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:01 AM").ToUniversalTime()
        };
        public static readonly OrderEntity PAGINATION_ACTIVE_SELL_ORDER1 = new OrderEntity
        {
            Id = "05d30bfd-26b8-4af5-84f1-2bc91553164b",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Sell,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:02 AM").ToUniversalTime()
        };
        public static readonly OrderEntity PAGINATION_ACTIVE_BUY_ORDER2 = new OrderEntity
        {
            Id = "bc4a68e6-7da7-4bf6-b976-c309121629de",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:03 AM").ToUniversalTime()
        };
        public static readonly OrderEntity PAGINATION_ACTIVE_SELL_ORDER2 = new OrderEntity
        {
            Id = "cb867e36-6b5d-4eef-ab7c-2200a12a5473",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Sell,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:04 AM").ToUniversalTime()
        };

        public static readonly OrderEntity PAGINATION_ACTIVE_BUY_ORDER3 = new OrderEntity
        {
            Id = "8765b84f-e23b-4cc4-9f6d-36e77833458d",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:05 AM").ToUniversalTime()
        };
        public static readonly OrderEntity PAGINATION_ACTIVE_SELL_ORDER3 = new OrderEntity
        {
            Id = "145cf9ae-c7d7-4c4e-b2ff-eac4589befd5",
            OwnerId = TESTUSER_ID,
            PortfolioId = PAGINATION_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Sell,
            OrderStatus = OrderStatus.Active,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("5/6/2021 08:00:06 AM").ToUniversalTime()
        };
    }
}