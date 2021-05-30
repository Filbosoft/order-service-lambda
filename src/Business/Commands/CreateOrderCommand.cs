using System;
using System.ComponentModel.DataAnnotations;
using Business.Validation.Attributes;
using Business.Wrappers;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;

namespace Business.Commands
{
    public class CreateOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string PortfolioId { get; set; }
        [Required]
        public OrderType? Type { get; set; }
        [Required]
        public string AssetSymbol { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }
        [DateTimeLaterThanUTCNowValidationAttribute]
        public DateTime? ExpiresAt { get; set; }
    }

    public enum CreateOrderResponseCodes
    {
        Success,
        PortfolioNotFound,
        AssetNotFound,
        ValidationFailed
    }
}