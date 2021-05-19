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
        public CreateOrderCommandHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<OrderEntity>(request);
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