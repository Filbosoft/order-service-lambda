using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class ValidateSellOrderRequest : IValidationRequest
    {
        [Required]
        public PortfolioDetail Portfolio { get; set; }
        [Required]
        public AssetDetail Asset { get; set; }
        [Required]
        [Range(1,int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class ValidateSellOrderRequestHandler : IValidationHandler<ValidateSellOrderRequest>
    {
        private readonly IMapper _mapper;

        public ValidateSellOrderRequestHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ValidationResult> Handle(ValidateSellOrderRequest request, CancellationToken cancellationToken)
        {
            var portfolioAsset = request.Portfolio.Assets
                .FirstOrDefault(a => a.Symbol.Equals(request.Asset.Symbol));

            if (portfolioAsset == null)
                return new ValidationResult($"Portfolio doesn't hold any {request.Asset.Name} assets");

            if (portfolioAsset.Quantity < request.Quantity)
                return new ValidationResult($"Portfolio has an insufficient quantity of {request.Asset.Name} assets to complete this order");

            return ValidationResult.Success;
        }
    }
}