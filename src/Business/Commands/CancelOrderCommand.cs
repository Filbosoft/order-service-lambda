using System;
using Business.Wrappers;
using Conditus.Trader.Domain.Models;

namespace Business.Commands
{
    public class CancelOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string OrderId { get; set; }
        public DateTime OrderCreatedAt { get; set; }
    }
}