using System;
using Business.Wrappers;
using Conditus.Trader.Domain.Models;

namespace Business.Commands
{
    public class UpdateOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string Id { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public bool Cancel { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
    }

    public enum UpdateOrderResponseCodes
    {
        Success,
        OrderNotFound,
        OrderNotActive,
        ValidationFailed,
        NoUpdatesFound
    }
}