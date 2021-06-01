using System.Collections.Generic;
using Business.Wrappers;
using System;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Business.Queries
{
    public class GetOrdersQuery : BusinessRequest, IRequestWrapper<IEnumerable<OrderOverview>>
    {
        public string PortfolioId { get; set; }
        public OrderType? Type { get; set; }
        public OrderStatus? Status { get; set; }
        public string AssetSymbol { get; set; }
        public AssetType? AssetType { get; set; }
        public DateTime? CreatedFromDate { get; set; }
        public DateTime? CreatedToDate { get; set; }
        public DateTime? CompletedFromDate { get; set; }
        public DateTime? CompletedToDate { get; set; }
        [Range(1, 100)]
        public int PageSize { get; set; } = 25;
        public string PaginationToken { get; set; }
    }

    public enum GetOrdersResponseCodes
    {
        Success
    }
}