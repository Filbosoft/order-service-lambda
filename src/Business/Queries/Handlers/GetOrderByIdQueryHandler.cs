using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using System.Linq;
using Conditus.Trader.Domain.Models;
using Amazon.DynamoDBv2.Model;
using Conditus.Trader.Domain.Entities;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;

namespace Business.Queries.Handlers
{
    public class GetOrderByIdQueryHandler : IHandlerWrapper<GetOrderByIdQuery, OrderDetail>
    {
        private readonly IAmazonDynamoDB _db;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IAmazonDynamoDB db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        /***
        * Query parameters
        ***/
        private const string V_ORDER_ID = ":v_order_id";
        private const string V_REQUESTING_USER_ID = ":v_requesting_user_id";
        public async Task<BusinessResponse<OrderDetail>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var query = new QueryRequest
            {
                TableName = typeof(OrderEntity).GetDynamoDBTableName(),
                Select = "ALL_ATTRIBUTES",
                IndexName = OrderLocalSecondaryIndexes.UserOrderIdIndex,
                KeyConditionExpression = $"{nameof(OrderEntity.OwnerId)} = {V_REQUESTING_USER_ID} AND {nameof(OrderEntity.Id)} = {V_ORDER_ID}",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_REQUESTING_USER_ID, new AttributeValue{S = request.RequestingUserId}},
                    {V_ORDER_ID, new AttributeValue{S = request.OrderId}}
                }
            };

            var response = await _db.QueryAsync(query);
            var orderEntity = response.Items
                .Select(i => i.ToEntity<OrderEntity>())
                .FirstOrDefault();
            
            if (orderEntity == null)
                return BusinessResponse.Fail<OrderDetail>(
                    GetOrderByIdResponseCodes.OrderNotFound,
                    $"No order with the id of {request.OrderId} was found");

            var orderDetail = _mapper.Map<OrderDetail>(orderEntity);

            return BusinessResponse.Ok<OrderDetail>(orderDetail);
        }
    }
}