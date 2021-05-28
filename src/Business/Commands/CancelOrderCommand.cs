using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Business.HelperMethods;
using Business.Wrappers;
using Conditus.DynamoDBMapper.Mappers;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;

namespace Business.Commands
{
    public class CancelOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string OrderId { get; set; }
        public DateTime OrderCreatedAt { get; set; }
    }

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
            var cancelRequest = GetCancelRequest(request);
            var response = await _db.UpdateItemAsync(cancelRequest);
            var updatedEntity = response.Attributes.ToEntity<OrderEntity>();
            var orderDetail = _mapper.Map<OrderDetail>(updatedEntity);

            return BusinessResponse.Ok(orderDetail, "Order updated");
        }

        /***
        * Expression attributes
        ***/
        private const string V_NEW_STATUS = ":v_new_status";

        public UpdateItemRequest GetCancelRequest(CancelOrderCommand request)
        {
            var cancelRequest = new UpdateItemRequest
            {
                TableName = DynamoDBHelper.GetDynamoDBTableName<OrderEntity>(),
                Key = new Dictionary<string, AttributeValue>
                {
                    {nameof(OrderEntity.OwnerId), request.RequestingUserId.GetAttributeValue()},
                    {nameof(OrderEntity.CreatedAt), request.OrderCreatedAt.GetAttributeValue()}
                },
                UpdateExpression = $"SET {nameof(OrderEntity.OrderStatus)} = {V_NEW_STATUS}",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_NEW_STATUS, OrderStatus.Cancelled.GetAttributeValue()}
                },
                ReturnValues = "ALL_NEW"
            };

            return cancelRequest;
        }
    }
}