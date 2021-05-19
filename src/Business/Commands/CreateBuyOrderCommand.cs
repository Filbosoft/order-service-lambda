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
    public class CreateBuyOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
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
        [Required]
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateBuyOrderCommandHandler : IHandlerWrapper<CreateBuyOrderCommand, OrderDetail>
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyRepository _currencyRepository;

        public CreateBuyOrderCommandHandler(IMapper mapper, ICurrencyRepository currencyRepository)
        {
            _mapper = mapper;
            _currencyRepository = currencyRepository;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(CreateBuyOrderCommand request, CancellationToken cancellationToken)
        {
            decimal orderCost;
            if (request.Asset.Currency.Symbol.Equals("DKK"))
                orderCost = request.Price * request.Quantity;
            else
                orderCost = await _currencyRepository.ConvertCurrency(request.Asset.Currency.Symbol, "DKK", request.Quantity);
            
            var diff = request.Portfolio.Capital - orderCost;

            if (diff < 0)
                return BusinessResponse.Fail<OrderDetail>("Insufficient capital to complete this order");

            var entity = _mapper.Map<OrderEntity>(request);
            entity.AssetName = request.Asset.Name;
            

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