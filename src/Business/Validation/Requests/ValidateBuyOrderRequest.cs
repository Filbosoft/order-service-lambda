using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using Business.Wrappers;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;
using MediatR;

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

        public async Task<ValidationResult> Handle(ValidateBuyOrderRequest request, CancellationToken cancellationToken)
        {
            decimal orderCost;
            if (request.Asset.Currency.Code.Equals("DKK"))
                orderCost = request.Price * request.Quantity;
            else
                orderCost = await _currencyRepository.ConvertCurrency(request.Asset.Currency.Code, "DKK", request.Quantity);
            
            var remainingCapital = request.Portfolio.Capital - orderCost;

            if (remainingCapital < 0)
                return new ValidationResult("Insufficient capital to complete this order");

            return ValidationResult.Success;
        }
    }
}