using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Entities;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;
using Business.Repositories;

namespace Business.Queries.Handlers
{
    public class GetOrderByIdQueryHandler : IHandlerWrapper<GetOrderByIdQuery, OrderDetail>
    {
        private readonly IAmazonDynamoDB _db;
        private readonly IMapper _mapper;
        private readonly IPortfolioRepository _portfolioRepository;

        public GetOrderByIdQueryHandler(IAmazonDynamoDB db, IMapper mapper, IPortfolioRepository portfolioRepository)
        {
            _db = db;
            _mapper = mapper;
            _portfolioRepository = portfolioRepository;
        }

        /***
        * Query parameters
        ***/
        private const string V_ORDER_ID = ":v_order_id";
        private const string V_REQUESTING_USER_ID = ":v_requesting_user_id";
        public async Task<BusinessResponse<OrderDetail>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                request.RequestingUserId.GetAttributeValue(),
                request.OrderId.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);
            
            if (entity == null)
                return BusinessResponse.Fail<OrderDetail>(
                    GetOrderByIdResponseCodes.OrderNotFound,
                    $"No order with the id of {request.OrderId} was found");

            var orderDetail = _mapper.Map<OrderDetail>(entity);
            var orderDetailWithJoins = await SetOrderJoinProperties(orderDetail);

            return BusinessResponse.Ok<OrderDetail>(orderDetail);
        }

        private async Task<OrderDetail> SetOrderJoinProperties(OrderDetail order)
        {
            var portfolio = await _portfolioRepository.GetPortfolioById(order.PortfolioId);
            if (portfolio != null) order.PortfolioName = portfolio.Name;

            return order;
        }
    }
}