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
        private readonly IMapper _mapper;
        private readonly ICurrencyRepository _currencyRepository;

        public ValidateBuyOrderRequestHandler(IMapper mapper, ICurrencyRepository currencyRepository)
        {
            _mapper = mapper;
            _currencyRepository = currencyRepository;
        }

        private const string DEFAULT_PORTFOLIO_CURRENCY_SYMBOL = "DKK";

        public async Task<ValidationResult> Handle(ValidateBuyOrderRequest request, CancellationToken cancellationToken)
        {
            decimal orderCost;
            if (request.Asset.Currency.Code.Equals(DEFAULT_PORTFOLIO_CURRENCY_SYMBOL))
                orderCost = request.Price * request.Quantity;
            else
                orderCost =  await _currencyRepository.ConvertCurrency(
                    request.Asset.Currency.Code,
                    DEFAULT_PORTFOLIO_CURRENCY_SYMBOL,
                    request.Quantity);
            
            var remainingCapital = request.Portfolio.Capital - orderCost;

            if (remainingCapital < 0)
                return new ValidationResult("Insufficient capital in portfolio to complete this order");

            return ValidationResult.Success;
        }
    }
}