using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;

namespace Business.Commands
{
    public class CreateOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string PortfolioId { get; set; }
        [Required]
        public OrderType Type { get; set; }
        [Required]
        public string AssetSymbol { get; set; }
        [Required]
        [Range(1,int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateOrderCommandHandler : IHandlerWrapper<CreateOrderCommand, OrderDetail>
    {
        private readonly IMapper _mapper;
        private readonly IAssetRepository _assetRepository;
        private readonly IPortfolioRepository _portfolioRepository;

        public CreateOrderCommandHandler(IMapper mapper, IAssetRepository assetRepository, IPortfolioRepository portfolioRepository)
        {
            _mapper = mapper;
            _assetRepository = assetRepository;
            _portfolioRepository = portfolioRepository;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var portfolio = await _portfolioRepository.GetPortfolioById(request.PortfolioId);
            if (portfolio == null)
                return BusinessResponse.Fail<OrderDetail>("Error occurred trying to get portfolio");

            var asset = await _assetRepository.GetAssetBySymbol(request.AssetSymbol);
            if (asset == null)
                return BusinessResponse.Fail<OrderDetail>("Error occurred trying to get asset");

            var entity = _mapper.Map<OrderEntity>(request);
            entity.AssetName = asset.Name;
            

            Order placedOrder;

            try
            {
                placedOrder = await _orderRepository.PlaceOrderAsync(request.PortfolioId, entity);
            } 
            catch (KeyNotFoundException)
            {
                return BusinessResponse.Fail<OrderDetail>($"No portfolio with the id of {request.PortfolioId} found");
            }
            
            return BusinessResponse.Ok<OrderDetail>(placedOrder, "Order created!");
        }
    }
}