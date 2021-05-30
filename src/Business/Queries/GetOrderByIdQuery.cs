using Business.Wrappers;
using Conditus.Trader.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Business.Queries
{
    public class GetOrderByIdQuery : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        [Required]
        public string OrderId { get; set; }
    }

    public enum GetOrderByIdResponseCodes
    {
        Success,
        OrderNotFound
    }
}