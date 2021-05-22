using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using System.Linq;
using System;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using Amazon.DynamoDBv2.Model;
using Conditus.Trader.Domain.Entities;
using Amazon.DynamoDBv2;
using Conditus.Trader.Domain;
using Database.Indexes;
using System.ComponentModel.DataAnnotations;

namespace Business.Queries
{
    public class GetOrderByIdQuery : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        [Required]
        public string OrderId { get; set; }
    }

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
                TableName = "Orders", // TODO: Figure out if the system should have a db table constants class in the Database project
                Select = "ALL_ATTRIBUTES",
                IndexName = LocalIndexes.UserOrderIdIndex,
                KeyConditionExpression = $"{nameof(OrderEntity.OwnerId)} = {V_REQUESTING_USER_ID} AND {nameof(OrderEntity.Id)} = {V_ORDER_ID}",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_REQUESTING_USER_ID, new AttributeValue{S = request.RequestingUserId}},
                    {V_ORDER_ID, new AttributeValue{S = request.OrderId}}
                }
            };

            var response = await _db.QueryAsync(query);
            var orderEntity = response.Items
                .Select(DynamoDBMapper.MapAttributeMapToEntity<OrderEntity>)
                .FirstOrDefault();
            
            if (orderEntity == null)
                return BusinessResponse.Fail<OrderDetail>($"No order with the id of {request.OrderId} was found");

            var orderDetail = _mapper.Map<OrderDetail>(orderEntity);

            return BusinessResponse.Ok<OrderDetail>(orderDetail);
        }
    }
}