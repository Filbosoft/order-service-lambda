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
using Conditus.DynamoDBMapper.Mappers;
using Business.HelperMethods;
using Amazon.DynamoDBv2.DataModel;

namespace Business.Queries.Handlers
{
    public class GetOrdersQueryHandler : IHandlerWrapper<GetOrdersQuery, IEnumerable<OrderOverview>>
    {
        private readonly IAmazonDynamoDB _db;
        private readonly IMapper _mapper;

        public GetOrdersQueryHandler(IAmazonDynamoDB db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        public async Task<BusinessResponse<IEnumerable<OrderOverview>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (request.CreatedFromDate == null)
                request.CreatedFromDate = DateTime.UtcNow.AddYears(-10);

            var query = GetQueryRequest(request);
            var response = await _db.QueryAsync(query);
            var orderEntities = response.Items
                .Select(i => i.ToEntity<OrderEntity>())
                .ToList();

            var orderOverviews = orderEntities.Select(_mapper.Map<OrderOverview>);

            return BusinessResponse.Ok<IEnumerable<OrderOverview>>(orderOverviews);
        }

        /***
        * Query conditions
        ***/
        private List<string> KeyConditions { get; set; } = new List<string>();
        private List<string> FilterConditions { get; set; } = new List<string>();
        private Dictionary<string, AttributeValue> ExpressionAttributeValues { get; set; } = new Dictionary<string, AttributeValue>();

        /***
        * Query parameters
        ***/
        private const string V_CREATED_FROM_DATE = ":v_created_from_date";
        private const string V_CREATED_TO_DATE = ":v_created_to_date";
        private const string V_TYPE = ":v_type";
        private const string V_STATUS = ":v_status";
        private const string V_PORTFOLIO_ID = ":v_portfolio_id";
        private const string V_ASSET_SYMBOL = ":v_asset_symbol";
        private const string V_ASSET_TYPE = ":v_asset_type";
        private const string V_COMPLETED_FROM_DATE = ":v_completed_from_date";
        private const string V_COMPLETED_TO_DATE = ":v_completed_to_date";
        private const string V_REQUESTING_USER_ID = ":v_requesting_user_id";

        private QueryRequest GetQueryRequest(GetOrdersQuery request)
        {
            var query = new QueryRequest
            {
                TableName = DynamoDBHelper.GetDynamoDBTableName<OrderEntity>(),
                Select = "ALL_ATTRIBUTES",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_CREATED_FROM_DATE, ((DateTime) request.CreatedFromDate).GetAttributeValue()}
                }
            };

            var index = GetOptimalOrderIndex(request);
            query.IndexName = index;

            SetQueryConditions(request, query);

            query.KeyConditionExpression = string.Join(" AND ", KeyConditions);

            if (FilterConditions.Count > 0)
                query.FilterExpression = string.Join(" AND ", FilterConditions);

            return query;
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

        private void SetQueryConditions(GetOrdersQuery request, QueryRequest query)
        {
            if (request.CreatedToDate != null)
            {
                var createdToCondition = $"({nameof(OrderEntity.CreatedAt)} BETWEEN {V_CREATED_FROM_DATE} AND {V_CREATED_TO_DATE})";
                AddIndexCondition(null, query.IndexName, createdToCondition);

                query.ExpressionAttributeValues.Add(
                    V_CREATED_TO_DATE,
                    ((DateTime)request.CreatedToDate).GetAttributeValue());
            }
            else
            {
                var createdFromCondition = $"({nameof(OrderEntity.CreatedAt)} >= {V_CREATED_FROM_DATE})";
                AddIndexCondition(null, query.IndexName, createdFromCondition);
            }

            if (request.Type != null)
            {
                AddIndexCondition(
                    LocalIndexes.UserOrderTypeIndex,
                    query.IndexName,
                    $"{nameof(OrderEntity.OrderType)} = {V_TYPE}");

                query.ExpressionAttributeValues.Add(
                    V_TYPE,
                    request.Type.GetAttributeValue());
            }

            if (request.Status != null)
            {
                AddIndexCondition(
                    LocalIndexes.UserOrderStatusIndex,
                    query.IndexName,
                    $"{nameof(OrderEntity.OrderStatus)} = {V_STATUS}");

                query.ExpressionAttributeValues.Add(
                    V_STATUS,
                    request.Status.GetAttributeValue());
                    
            }

            if (request.PortfolioId != null)
            {
                AddIndexCondition(
                    LocalIndexes.UserOrderPortfolioIndex,
                    query.IndexName,
                    $"{nameof(OrderEntity.PortfolioId)} = {V_PORTFOLIO_ID}");

                query.ExpressionAttributeValues.Add(
                    V_PORTFOLIO_ID,
                    new AttributeValue { S = request.PortfolioId });
            }

            if (request.AssetSymbol != null)
            {
                AddIndexCondition(
                    LocalIndexes.UserOrderAssetIndex,
                    query.IndexName,
                    $"{nameof(OrderEntity.AssetSymbol)} = {V_ASSET_SYMBOL}");

                query.ExpressionAttributeValues.Add(
                    V_ASSET_SYMBOL,
                    new AttributeValue { S = request.AssetSymbol });
            }

            if (request.AssetType != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.AssetType)} = {V_ASSET_TYPE}");
                query.ExpressionAttributeValues.Add(
                    V_ASSET_TYPE,
                    request.AssetType.GetAttributeValue());
            }

            if (request.CompletedFromDate != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.CompletedAt)} >= {V_COMPLETED_FROM_DATE}");
                query.ExpressionAttributeValues.Add(
                    V_COMPLETED_FROM_DATE,
                    ((DateTime)request.CompletedFromDate).GetAttributeValue());
            }

            if (request.CompletedToDate != null)
            {
                FilterConditions.Add($"{nameof(OrderEntity.CompletedAt)} <= {V_COMPLETED_TO_DATE}");
                query.ExpressionAttributeValues.Add(
                    V_COMPLETED_TO_DATE,
                    ((DateTime)request.CompletedToDate).GetAttributeValue());
            }

            KeyConditions.Add($"({nameof(OrderEntity.OwnerId)} = {V_REQUESTING_USER_ID})");
            query.ExpressionAttributeValues.Add(
                V_REQUESTING_USER_ID,
                new AttributeValue { S = request.RequestingUserId });
        }

        private void AddIndexCondition(object attribute, QueryRequest query, string expectedIndex, string condition, string conditionKey )
        {
            AddIndexCondition(
                    expectedIndex,
                    query.IndexName,
                    condition);

            query.ExpressionAttributeValues.Add(
                conditionKey,
                attribute.GetAttributeValue());
        }

        /***
        * AddIndexCondition adds the condition to either the KeyConditions or the FilterConditions
        * based on the selected index for the query.
        ***/
        private void AddIndexCondition(string expectedIndex, string actualIndex, string condition)
        {
            if (actualIndex == expectedIndex //If the indexes are null, Equals will throw a NullReferenceException
                || actualIndex.Equals(expectedIndex))
                    KeyConditions.Add(condition);
                else FilterConditions.Add(condition);
        }
    }
}