using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Business.Extensions;
using Business.HelperMethods;
using Business.Validation.Requests;
using Business.Wrappers;
using Conditus.DynamoDBMapper.Mappers;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;
using Database.Indexes;
using MediatR;

namespace Business.Commands
{
    public class UpdateOrderCommand : BusinessRequest, IRequestWrapper<OrderDetail>
    {
        public string Id { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public bool Cancel { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateOrderCommandHandler : IHandlerWrapper<UpdateOrderCommand, OrderDetail>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAmazonDynamoDB _db;

        public UpdateOrderCommandHandler(
            IMediator mediator,
            IMapper mapper,
            IAmazonDynamoDB db)
        {
            _mediator = mediator;
            _mapper = mapper;
            _db = db;
        }

        public async Task<BusinessResponse<OrderDetail>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.LoadByLocalIndexAsync<OrderEntity>(
                request.RequestingUserId,
                nameof(OrderEntity.Id),
                request.Id,
                LocalIndexes.UserOrderIdIndex
            );

            if (entity == null)
                return BusinessResponse.Fail<OrderDetail>($"No order with the id of {request.Id} was found");

            if (!entity.OrderStatus.Equals(OrderStatus.Active))
                return BusinessResponse.Fail<OrderDetail>($"Can only update active orders");

            if (request.Cancel)
            {
                var cancelCommand = new CancelOrderCommand
                {
                    OrderId = request.Id,
                    OrderCreatedAt = entity.CreatedAt
                };
                var cancelResponse = await _mediator.Send(cancelCommand);
                
                return cancelResponse;
            }

            var validationRequest = _mapper.Map<ValidateUpdateOrderRequest>(request);
            var validationResult = await _mediator.Send(validationRequest);

            if (validationResult != ValidationResult.Success)
                return BusinessResponse.Fail<OrderDetail>(validationResult.ErrorMessage);

            var updateRequest = GetUpdateRequest(request, entity);
            if (updateRequest == null)
                return BusinessResponse.Fail<OrderDetail>("No updates was found");

            var response = await _db.UpdateItemAsync(updateRequest);
            var updatedEntity = response.Attributes.ToEntity<OrderEntity>();
            var orderDetail = _mapper.Map<OrderDetail>(updatedEntity);
            
            return BusinessResponse.Ok<OrderDetail>(orderDetail, "Order updated!");
        }

        /***
        * Expression attributes
        ***/

        public const string V_NEW_QUANTITY = ":v_new_quantity";
        public const string V_NEW_PRICE = ":v_new_price";
        public const string V_NEW_EXPIRES_AT = ":v_new_expires_at";
        

        public UpdateItemRequest GetUpdateRequest(UpdateOrderCommand request, OrderEntity entity)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = DynamoDBHelper.GetDynamoDBTableName<OrderEntity>(),
                Key = new Dictionary<string, AttributeValue>
                {
                    {nameof(OrderEntity.OwnerId), request.RequestingUserId.GetAttributeValue()},
                    {nameof(OrderEntity.CreatedAt), entity.CreatedAt.GetAttributeValue()}
                },
                ReturnValues = "ALL_NEW"
            };
            var updateExpressions = new List<string>();
            var attributeValues = new Dictionary<string, AttributeValue>();

            if (request.Quantity != null)
            {
                updateExpressions.Add($"SET {nameof(OrderEntity.Quantity)} = {V_NEW_QUANTITY}");
                attributeValues.Add(V_NEW_QUANTITY, request.Quantity.GetAttributeValue());
            }

            if (request.Price != null)
            {
                updateExpressions.Add($"SET {nameof(OrderEntity.Price)} = {V_NEW_PRICE}");
                attributeValues.Add(V_NEW_PRICE, request.Price.GetAttributeValue());
            }

            if (request.ExpiresAt != null)
            {
                updateExpressions.Add($"SET {nameof(OrderEntity.ExpiresAt)} = {V_NEW_EXPIRES_AT}");
                attributeValues.Add(V_NEW_EXPIRES_AT, request.ExpiresAt.GetAttributeValue());
            }

            if (updateExpressions.Count == 0)
                return null;

            updateRequest.UpdateExpression = String.Join(' ', updateExpressions);
            updateRequest.ExpressionAttributeValues = attributeValues;

            return updateRequest;
        }

        
    }
}