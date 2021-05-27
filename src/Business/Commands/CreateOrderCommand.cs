using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using Business.Repositories;
using Business.Validation.Attributes;
using Business.Validation.Requests;
using Business.Wrappers;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;
using MediatR;

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
        public decimal Price { get; set; }
        [DateTimeLaterThanUTCNowValidationAttribute]
        public DateTime? ExpiresAt { get; set; }
    }

    public class CreateOrderCommandHandler : IHandlerWrapper<CreateOrderCommand, OrderDetail>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAssetRepository _assetRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IDynamoDBContext _dbContext;

        public CreateOrderCommandHandler(
            IMediator mediator,
            IMapper mapper,
            IAssetRepository assetRepository,
            IPortfolioRepository portfolioRepository,
            IDynamoDBContext dbContext)
        {
            _mediator = mediator;
            _mapper = mapper;
            _assetRepository = assetRepository;
            _portfolioRepository = portfolioRepository;
            _dbContext = dbContext;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var portfolio = await _portfolioRepository.GetPortfolioById(request.PortfolioId);
            if (portfolio == null)
                return BusinessResponse.Fail<OrderDetail>("Error occurred trying to get portfolio");

            var asset = await _assetRepository.GetAssetBySymbol(request.AssetSymbol);
            if (asset == null)
                return BusinessResponse.Fail<OrderDetail>("Error occurred trying to get asset");

            IValidationRequest validationRequest = GetValidationRequest(request, portfolio, asset);
            var validationResult = await _mediator.Send(validationRequest);

            if (validationResult != ValidationResult.Success)
                return BusinessResponse.Fail<OrderDetail>(validationResult.ErrorMessage);

            var entity = _mapper.Map<OrderEntity>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.AssetName = asset.Name;
            if (request.ExpiresAt == null)
                entity.ExpiresAt = DateTime.UtcNow.AddDays(1);

            await _dbContext.SaveAsync(entity);

            var orderDetail = _mapper.Map<OrderDetail>(entity);
            return BusinessResponse.Ok<OrderDetail>(orderDetail, "Order created!");
        }

        public IValidationRequest GetValidationRequest(CreateOrderCommand request, PortfolioDetail portfolio, AssetDetail asset)
        {
            switch (request.Type)
            {
                case OrderType.Buy:
                    return new ValidateBuyOrderRequest
                    {
                        Portfolio = portfolio,
                        Asset = asset,
                        Price = request.Price,
                        Quantity = request.Quantity
                    };
                case OrderType.Sell:
                    return new ValidateSellOrderRequest
                    {
                        Portfolio = portfolio,
                        Asset = asset,
                        Quantity = request.Quantity
                    };
                default:
                    return null;
            }
        }
    }
}