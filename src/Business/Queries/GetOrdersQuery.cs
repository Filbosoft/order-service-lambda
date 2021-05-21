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

        private List<string> KeyConditions { get; set; } = new List<string>();
        private List<string> FilterConditions { get; set; } = new List<string>();

        public async Task<BusinessResponse<IEnumerable<OrderOverview>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (request.CreatedFromDate == null)
                request.CreatedFromDate = DateTime.UtcNow.AddYears(-10);

            var query = new QueryRequest
            {
                TableName = "Orders",
                Select = "ALL_ATTRIBUTES",
                ScanIndexForward = true,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_created_from_date", new AttributeValue{ N = DynamoDBMapper.GetUnixTimeMSFromDateTime((DateTime) request.CreatedFromDate).ToString()}}
                }
            };

            var index = GetOptimalOrderIndex(request);
            query.IndexName = index;

            if (request.CreatedToDate != null)
            {
                var createdToCondition = $"({nameof(OrderEntity.CreatedAt)} BETWEEN :v_created_from_date AND :v_created_to_date)";

                if (index == null) 
                    KeyConditions.Add(createdToCondition);
                else FilterConditions.Add(createdToCondition);

                query.ExpressionAttributeValues.Add(
                    ":v_created_to_date",
                    new AttributeValue { N = DynamoDBMapper.GetUnixTimeMSFromDateTime((DateTime)request.CreatedToDate).ToString() });
            }
            else 
            {
                var createdFromCondition = $"({nameof(OrderEntity.CreatedAt)} >= :v_created_from_date)";
                if (index == null)
                    KeyConditions.Add(createdFromCondition);
                else FilterConditions.Add(createdFromCondition);
            } 

            if (request.Type != null)
            {
                var typeCondition = $"{nameof(OrderEntity.OrderType)} = :v_type";

                if (index.Equals(LocalIndexes.UserOrderTypeIndex))
                    KeyConditions.Add(typeCondition);
                else FilterConditions.Add(typeCondition);

                query.ExpressionAttributeValues.Add(
                    ":v_type",
                    new AttributeValue { N = ((int)request.Type).ToString() });
            }

            if (request.Status != null)
            {
                var statusCondition = $"{nameof(OrderEntity.OrderStatus)} = :v_status";

                if (index.Equals(LocalIndexes.UserOrderStatusIndex))
                    KeyConditions.Add(statusCondition);
                else FilterConditions.Add(statusCondition);

                query.ExpressionAttributeValues.Add(
                    ":v_status",
                    new AttributeValue { N = ((int)request.Status).ToString() });
            }

            if (request.PortfolioId != null)
            {
                var portfolioIdCondition = $"{nameof(OrderEntity.PortfolioId)} = :v_portfolio_id";

                if (index.Equals(LocalIndexes.UserOrderPortfolioIndex))
                    KeyConditions.Add(portfolioIdCondition);
                else FilterConditions.Add(portfolioIdCondition);

                query.ExpressionAttributeValues.Add(
                    ":v_portfolio_id",
                    new AttributeValue { S = request.PortfolioId });
            }

            if (request.AssetSymbol != null)
            {
                var assetSymbolCondition = $"{nameof(OrderEntity.AssetSymbol)} = :v_asset_symbol";

                if (index.Equals(LocalIndexes.UserOrderAssetIndex))
                    KeyConditions.Add(assetSymbolCondition);
                else FilterConditions.Add(assetSymbolCondition);

                query.ExpressionAttributeValues.Add(
                    ":v_asset_symbol",
                    new AttributeValue { S = request.AssetSymbol });
            }

            if (request.AssetType != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.AssetType)} = :v_asset_type");
                query.ExpressionAttributeValues.Add(
                    ":v_asset_type",
                    new AttributeValue { N = ((int)request.Status).ToString() });
            }

            if (request.CompletedFromDate != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.CompletedAt)} >= :v_completed_from_date");
                query.ExpressionAttributeValues.Add(
                    ":v_completed_from_date",
                    new AttributeValue { N = DynamoDBMapper.GetUnixTimeMSFromDateTime((DateTime)request.CompletedFromDate).ToString() });
            }

            if (request.CompletedToDate != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.CompletedAt)} <= :v_completed_to_date");
                query.ExpressionAttributeValues.Add(
                    ":v_completed_to_date",
                    new AttributeValue { N = DynamoDBMapper.GetUnixTimeMSFromDateTime((DateTime)request.CompletedToDate).ToString() });
            }

            KeyConditions.Add($"({nameof(OrderEntity.CreatedBy)} = :v_requesting_user_id)");
            query.ExpressionAttributeValues.Add(
                ":v_requesting_user_id",
                new AttributeValue { S = request.RequestingUserId });

            query.KeyConditionExpression = string.Join(" AND ", KeyConditions);
            
            if (FilterConditions.Count > 0)
                query.FilterExpression = string.Join(" AND ", FilterConditions);

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

        /***
        * This function returns the index name of the index that will result in the least amount of records.
        * As it's only possible to query one index at a time, it's important to choose the optimal one for the query.
        ***/
        public string GetOptimalOrderIndex(GetOrdersQuery request)
        {
            if (request.AssetSymbol != null)
                return LocalIndexes.UserOrderAssetIndex;

            if (request.PortfolioId != null)
                return LocalIndexes.UserOrderPortfolioIndex;

            if (request.Status != null)
                return LocalIndexes.UserOrderStatusIndex;

            if (request.Type != null)
                return LocalIndexes.UserOrderTypeIndex;

            return null;
        }

        public void SetCreatedFromSortKeyExpression(GetOrdersQuery request, QueryRequest query)
        {
            
        }
    }
}