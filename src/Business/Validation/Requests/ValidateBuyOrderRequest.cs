using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Repositories;
using Business.Wrappers;
using Conditus.Trader.Domain.Models;

namespace Business.Validation.Requests
{
    public class ValidateBuyOrderRequest : IValidationRequest
    {
        [Required]
        public PortfolioDetail Portfolio { get; set; }
        [Required]
        public AssetDetail Asset { get; set; }
        [Required]
        [Range(1,int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
    }

    public class ValidateBuyOrderRequestHandler : IValidationHandler<ValidateBuyOrderRequest>
    {
        private readonly ICurrencyRepository _currencyRepository;

        public ValidateBuyOrderRequestHandler(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<ValidationResult> Handle(ValidateBuyOrderRequest request, CancellationToken cancellationToken)
        {
            var assetCurrency = request.Asset.Currency.Code;
            var portfolioCurrency = request.Portfolio.CurrencyCode;

            decimal orderCostInPortfolioCurrency = assetCurrency.Equals(portfolioCurrency) ?
                request.Price * request.Quantity
                : await _currencyRepository.ConvertCurrency(
                    assetCurrency,
                    portfolioCurrency,
                    request.Price * request.Quantity);
            
            var remainingCapital = request.Portfolio.Capital - orderCostInPortfolioCurrency;

            if (remainingCapital < 0)
                return new ValidationResult("Insufficient capital in portfolio to complete this order");

            return ValidationResult.Success;
        }
    }
}