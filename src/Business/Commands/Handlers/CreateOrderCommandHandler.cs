using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using Business.Repositories;
using Business.Validation.Requests;
using Business.Wrappers;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;
using MediatR;

namespace Business.Commands.Handlers
{
    public class CreateOrderCommandHandler : IHandlerWrapper<CreateOrderCommand, OrderDetail>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAssetRepository _assetRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IAmazonDynamoDB _db;

        public CreateOrderCommandHandler(
            IMediator mediator,
            IMapper mapper,
            IAssetRepository assetRepository,
            IPortfolioRepository portfolioRepository,
            IAmazonDynamoDB db)
        {
            _mediator = mediator;
            _mapper = mapper;
            _assetRepository = assetRepository;
            _portfolioRepository = portfolioRepository;
            _db = db;
        }

        private const string DEFAULT_PORTFOLIO_CURRENCY_CODE = "DKK";

        public async Task<BusinessResponse<OrderDetail>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var portfolio = await _portfolioRepository.GetPortfolioById(request.PortfolioId);
            if (portfolio == null)
                return BusinessResponse.Fail<OrderDetail>(
                    CreateOrderResponseCodes.PortfolioNotFound,
                    "Error occurred trying to get portfolio");

            var asset = await _assetRepository.GetAssetBySymbol(request.AssetSymbol);
            if (asset == null)
                return BusinessResponse.Fail<OrderDetail>(
                    CreateOrderResponseCodes.AssetNotFound,
                    "Error occurred trying to get asset");

            IValidationRequest validationRequest = GetValidationRequest(request, portfolio, asset);
            var validationResult = await _mediator.Send(validationRequest);

            if (validationResult != ValidationResult.Success)
                return BusinessResponse.Fail<OrderDetail>(
                    CreateOrderResponseCodes.ValidationFailed,
                    validationResult.ErrorMessage);

            var entity = _mapper.Map<OrderEntity>(request);
            entity.Id = Guid.NewGuid().ToString();
            entity.AssetName = asset.Name;
            
            if (request.ExpiresAt == null)
                entity.ExpiresAt = DateTime.UtcNow.AddDays(1);

            var response = await _db.PutItemAsync(typeof(OrderEntity).GetDynamoDBTableName(), entity.GetAttributeValueMap());

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