using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;

using static Acceptance.Utilities.TestConstants;
using static Acceptance.Seeds.PortfolioSeeds;
using static Acceptance.Seeds.AssetSeeds;

namespace Acceptance.Seeds
{
    public static class OrderSeeds
    {        
        public static readonly OrderEntity COMPLETED_BUY_ORDER = new OrderEntity
        {
            Id = "e365a51c-b176-494f-8506-1c80cb84a69b",
            OwnerId = TESTUSER_ID,
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = TESTUSER_PORTFOLIO.Id,
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
            PortfolioId = NONTESTUSER_PORTFOLIO.Id,
            AssetSymbol = DKK_STOCK.Symbol,
            AssetType = DKK_STOCK.Type,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:06 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
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
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:07 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity COMPLETED_NONEXISTING_ASSET_ORDER = new OrderEntity
        {
            Id = "2eb225a6-035c-462a-a781-ce8d8cf2d08c",
            OwnerId = TESTUSER_ID,
            PortfolioId = TESTUSER_PORTFOLIO.Id,
            AssetSymbol = "NonExistingAsset",
            AssetType = AssetType.Stock,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = Convert.ToDateTime("2/5/2021 08:00:08 AM").ToUniversalTime(),
            CompletedAt = Convert.ToDateTime("3/5/2021 08:00:00 AM").ToUniversalTime()
        };

        public static readonly OrderEntity ORDER_COMPLETED_TODAY = new OrderEntity
        {
            Id = "ce6ac2d2-f42a-4df0-8953-ae2be90f8c8d",
            OwnerId = TESTUSER_ID,
            PortfolioId = TESTUSER_PORTFOLIO.Id,
            AssetSymbol = "NonExistingAsset",
            AssetType = AssetType.Stock,
            OrderType = OrderType.Buy,
            OrderStatus = OrderStatus.Completed,
            Price = 100M,
            Quantity = 10,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow.AddMinutes(-1)
        };
    }
}