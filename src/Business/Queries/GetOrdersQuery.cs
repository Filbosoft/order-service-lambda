using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.Enums;
using Amazon.DynamoDBv2.Model;
using Database;
using Conditus.Trader.Domain.Entities;
using Amazon.DynamoDBv2;
using Conditus.Trader.Domain;

namespace Business.Queries
{
    public class GetOrdersQuery : BusinessRequest, IRequestWrapper<IEnumerable<OrderOverview>>
    {
        public string PortfolioId { get; set; }
        public OrderType? Type { get; set; }
        public OrderStatus? Status { get; set; }
        public string AssetSymbol { get; set; }
        public AssetType? AssetType { get; set; }
        public DateTime? CreatedFromDate { get; set; }
        public DateTime? CreatedToDate { get; set; }
        public DateTime? CompletedFromDate { get; set; }
        public DateTime? CompletedToDate { get; set; }
    }

    public class GetOrdersQueryHandler : IHandlerWrapper<GetOrdersQuery, IEnumerable<OrderOverview>>
    {
        private readonly IAmazonDynamoDB _db;
        private readonly IMapper _mapper;

        public GetOrdersQueryHandler(IAmazonDynamoDB db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private string KeyExpression { get; set; }
        private List<string> FilterExpressions { get; set; } = new List<string>();

        public async Task<BusinessResponse<IEnumerable<OrderOverview>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (request.CreatedFromDate == null)
                request.CreatedFromDate = DateTime.UtcNow.AddYears(-10);

            var query = new QueryRequest
            {
                TableName = "Orders",
                Select = "ALL_ATTRIBUTES",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_created_from_date", new AttributeValue{ S = request.CreatedFromDate.ToString()}}
                }
            };

            var index = GetOptimalOrderIndex(request);
            var defaultExpression = $"{nameof(OrderEntity.CreatedAt)} >= :v_created_from_date";
            if (index != null)
            {
                query.IndexName = index;
                FilterExpressions.Add(defaultExpression);
            } else KeyExpression = defaultExpression;

            

            if (request.Type != null)
            {
                KeyExpression = $"{nameof(OrderEntity.Type)} = :v_type";
                if (!index.Equals(LocalIndexes.OrderTypeIndex)) FilterExpressions.Add(KeyExpression);

                query.ExpressionAttributeValues.Add(
                    ":v_type",
                    new AttributeValue{N = ((int)request.Type).ToString()});
            }

            if (request.Status != null)
            {
                KeyExpression = $"{nameof(OrderEntity.Status)} = :v_status";
                if (!index.Equals(LocalIndexes.OrderStatusIndex)) FilterExpressions.Add(KeyExpression);

                query.ExpressionAttributeValues.Add(
                    ":v_status",
                    new AttributeValue{N = ((int)request.Status).ToString()});
            }

            if (request.PortfolioId != null)
            {
                KeyExpression = $"{nameof(OrderEntity.PortfolioId)} = :v_portfolio_id";
                if (!index.Equals(LocalIndexes.OrderPortfolioIndex)) FilterExpressions.Add(KeyExpression);

                query.ExpressionAttributeValues.Add(
                    ":v_portfolio_id",
                    new AttributeValue{S = request.PortfolioId});
            }

            if (request.AssetSymbol != null)
            {
                KeyExpression = $"{nameof(OrderEntity.AssetSymbol)} = :v_asset_symbol";
                if (!index.Equals(LocalIndexes.OrderAssetIndex)) FilterExpressions.Add(KeyExpression);

                query.ExpressionAttributeValues.Add(
                    ":v_asset_symbol",
                    new AttributeValue{S = request.AssetSymbol});
            }

            if (request.AssetType != null)
            {
                FilterExpressions.Add($"{nameof(OrderEntity.AssetType)} = :v_asset_type");
                query.ExpressionAttributeValues.Add(
                    ":v_asset_type",
                    new AttributeValue{N = ((int)request.Status).ToString()});
            }

            if (request.CreatedToDate != null)
            {
                FilterExpressions.Add($"{nameof(OrderEntity.CreatedAt)} <= :v_created_to_date");
                query.ExpressionAttributeValues.Add(
                    ":v_created_to_date",
                    new AttributeValue{S = request.CreatedToDate.ToString()});
            }

            if (request.CompletedFromDate != null)
            {
                FilterExpressions.Add($"{nameof(OrderEntity.CompletedAt)} >= :v_completed_from_date");
                query.ExpressionAttributeValues.Add(
                    ":v_completed_from_date",
                    new AttributeValue{S = request.CompletedFromDate.ToString()});
            }

            if (request.CompletedToDate != null)
            {
                FilterExpressions.Add($"{nameof(OrderEntity.CompletedAt)} <= :v_completed_to_date");
                query.ExpressionAttributeValues.Add(
                    ":v_completed_to_date",
                    new AttributeValue{S = request.CompletedToDate.ToString()});
            }

            query.KeyConditionExpression = $"{nameof(OrderEntity.CreatedBy)} = :v_requesting_user_id and {KeyExpression}";
            query.ExpressionAttributeValues.Add(
                ":v_requesting_user_id",
                new AttributeValue{S = request.RequestingUserId});
            
            if (FilterExpressions.Count > 0)
                query.FilterExpression = string.Join(" and ", FilterExpressions);

            var response = await _db.QueryAsync(query);
            var orderItems = response.Items;
            var orderEntities = new List<OrderEntity>();

            foreach (var orderItem in orderItems)
            {
                var orderEntity = DynamoDBMapper.MapAttributeMapToEntity<OrderEntity>(orderItem);
                orderEntities.Add(orderEntity);
            }

            var orderOverviews = orderEntities.Select(_mapper.Map<OrderOverview>);

            return BusinessResponse.Ok<IEnumerable<OrderOverview>>(orderOverviews);
        }

        public string GetOptimalOrderIndex(GetOrdersQuery request)
        {
            if (request.AssetSymbol != null)
                return LocalIndexes.OrderAssetIndex;

            if (request.PortfolioId != null)
                return LocalIndexes.OrderPortfolioIndex;

            if (request.Status != null)
                return LocalIndexes.OrderStatusIndex;

            if (request.Type != null)
                return LocalIndexes.OrderTypeIndex;
            
            return null;
        }
    }
}