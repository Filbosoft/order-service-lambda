using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Business.Wrappers;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;

namespace Business.Commands.Handlers
{
    public class CancelOrderCommandHandler : IHandlerWrapper<CancelOrderCommand, OrderDetail>
    {
        private readonly IMapper _mapper;
        private readonly IAmazonDynamoDB _db;

        public CancelOrderCommandHandler(
            IMapper mapper,
            IAmazonDynamoDB db)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.LoadByLocalSecondaryIndexAsync<OrderEntity>(
                request.RequestingUserId.GetAttributeValue(),
                request.OrderId.GetAttributeValue(),
                OrderLocalSecondaryIndexes.UserOrderIdIndex);
            
            var cancelRequest = GetCancelRequest(request, entity);
            var response = await _db.UpdateItemAsync(cancelRequest);
            var updatedEntity = response.Attributes.ToEntity<OrderEntity>();
            var orderDetail = _mapper.Map<OrderDetail>(updatedEntity);

            return BusinessResponse.Ok(orderDetail, "Order updated");
        }

        /***
        * Expression attributes
        ***/
        private const string V_NEW_STATUS = ":v_new_status";
        private const string V_NEW_COMPOSITE_STATUS = ":v_new_composite_status";

        public UpdateItemRequest GetCancelRequest(CancelOrderCommand request, OrderEntity entity)
        {
            entity.OrderStatus = OrderStatus.Cancelled;

            var cancelRequest = new UpdateItemRequest
            {
                TableName = typeof(OrderEntity).GetDynamoDBTableName(),
                Key = new Dictionary<string, AttributeValue>
                {
                    {nameof(OrderEntity.OwnerId), request.RequestingUserId.GetAttributeValue()},
                    {nameof(OrderEntity.CreatedAt), request.OrderCreatedAt.GetAttributeValue()}
                },
                UpdateExpression = $"SET {nameof(OrderEntity.OrderStatus)} = {V_NEW_STATUS}, {nameof(OrderEntity.OrderStatusCreatedAtCompositeKey)} = {V_NEW_COMPOSITE_STATUS}",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_NEW_STATUS, entity.OrderStatus.GetAttributeValue()},
                    {V_NEW_COMPOSITE_STATUS, CompositeKeyMapper.GetCompositeKeyAttributeValue(entity, nameof(entity.OrderStatusCreatedAtCompositeKey))}
                },
                ReturnValues = "ALL_NEW"
            };

            return cancelRequest;
        }
    }
}