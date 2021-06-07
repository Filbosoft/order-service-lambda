using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Business.Wrappers;
using System.Linq;
using System;
using Conditus.Trader.Domain.Models;
using Amazon.DynamoDBv2.Model;
using Conditus.Trader.Domain.Entities;
using Amazon.DynamoDBv2;
using Conditus.DynamoDB.QueryExtensions.Pagination;
using Conditus.DynamoDB.QueryExtensions.Extensions;
using Conditus.DynamoDB.MappingExtensions.Mappers;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;
using Conditus.DynamoDB.MappingExtensions.Constants;
using Business.Repositories;

namespace Business.Queries.Handlers
{
    public class GetOrdersQueryHandler : IHandlerWrapper<GetOrdersQuery, IEnumerable<OrderOverview>>
    {
        private readonly IAmazonDynamoDB _db;
        private readonly IMapper _mapper;
        private readonly IPortfolioRepository _portfolioRepository;

        public GetOrdersQueryHandler(IAmazonDynamoDB db, IMapper mapper, IPortfolioRepository portfolioRepository)
        {
            _db = db;
            _mapper = mapper;
            _portfolioRepository = portfolioRepository;
        }

        private const int DEFAULT_MONTHS_AMOUNT = -1; //Negative to show it's one month backwards

        public async Task<BusinessResponse<IEnumerable<OrderOverview>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (request.CreatedFromDate == null)
                request.CreatedFromDate = GetDefaultCreatedFromDate(request);
            
            var query = GetQueryRequest(request);
            var paginatedQueryResponse = await QueryPaginatedAsync(query, request.PageSize);
            var orderOverviews = paginatedQueryResponse.Items
                .Select(m => m.ToEntity<OrderEntity>())
                .Select(_mapper.Map<OrderOverview>)
                .ToList();
            var orderOverviewsWithJoins = await SetOrdersJoinProperties(orderOverviews);
            
            var pagination = GetPaginationFromQueryResponse(paginatedQueryResponse);

            return BusinessResponse.Ok<IEnumerable<OrderOverview>>(orderOverviewsWithJoins, pagination);
        }

        private DateTime GetDefaultCreatedFromDate(GetOrdersQuery request)
        {
            if (request.CreatedToDate != null)
                return ((DateTime)request.CreatedToDate).AddMonths(DEFAULT_MONTHS_AMOUNT);

            if (request.CompletedFromDate != null)
                return ((DateTime) request.CompletedFromDate).AddDays(-1);

            if (request.CompletedToDate != null)
                return ((DateTime) request.CompletedToDate)
                    .AddMonths(DEFAULT_MONTHS_AMOUNT)
                    .AddDays(-1);
            
            return DateTime.UtcNow.AddMonths(DEFAULT_MONTHS_AMOUNT);
        }

        /// <summary>
        /// QueryPaginatedAsync:
        ///   - Because a QueryRequest with Limit and FilterExpression set may return less items than the limit,
        ///     as the filter is done post query, this function ensures the response includes a full page worth of items
        ///     or the rest of the items in the index.
        ///   - To ensure the LastEvaluatedKey is the last key in the index that was returned,
        ///     the function checks if the scanned count is bigger than the page size. If so the LastEvaluatedKey
        ///     will be the key of the last map in the page maps. If not the LastEvaluatedKey of the last query response will be used.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// A QueryResponse with the following properties set:
        ///     Items: Item maps (Dictionary<string, AttributeValue>)
        ///     LastEvaluatedKey: The key of the last scanned item in the index within the limit (Dictionary<string, AttributeValue>)
        ///     Count: The amount of items in the response (int)
        ///     ScannedCount: The amount of items returned over all the queries (int)
        /// </returns>
        private async Task<QueryResponse> QueryPaginatedAsync(QueryRequest query, int pageSize)
        {
            QueryResponse queryResponse = null;
            List<Dictionary<string, AttributeValue>> orderMaps = new List<Dictionary<string, AttributeValue>>();

            do
            {
                queryResponse = await _db.QueryAsync(query);
                orderMaps.AddRange(queryResponse.Items);
                query.ExclusiveStartKey = queryResponse.LastEvaluatedKey;
            } while (queryResponse.LastEvaluatedKey.Count > 0 && orderMaps.Count < pageSize);

            var isResultingItemsCountBiggerThanPageSize = orderMaps.Count > pageSize;
            var resultPageSize = isResultingItemsCountBiggerThanPageSize ? pageSize : orderMaps.Count;
            var pageOrderMaps = isResultingItemsCountBiggerThanPageSize ? orderMaps.GetRange(0, resultPageSize) : orderMaps;
            var paginatedQueryResponse = new QueryResponse
            {
                Items = pageOrderMaps,
                LastEvaluatedKey = GetPaginatedQueryLastEvaluatedKey(isResultingItemsCountBiggerThanPageSize, pageOrderMaps, queryResponse.LastEvaluatedKey),
                Count = resultPageSize,
                ScannedCount = orderMaps.Count
            };

            return paginatedQueryResponse;
        }

        private Dictionary<string, AttributeValue> GetPaginatedQueryLastEvaluatedKey(
            bool isResultingItemsCountBiggerThanPageSize,
            List<Dictionary<string, AttributeValue>> pageOrderMaps,
            Dictionary<string, AttributeValue> lastQueryResponseLastEvaluatedKey)

            => isResultingItemsCountBiggerThanPageSize ? 
                    GetOrderLastEvaluatedKey(pageOrderMaps.Last(), lastQueryResponseLastEvaluatedKey.Keys.ToList()) 
                    : lastQueryResponseLastEvaluatedKey;

        private Dictionary<string, AttributeValue> GetOrderLastEvaluatedKey(Dictionary<string, AttributeValue> orderAttributeMap, List<string> keys)
        {
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>();

            foreach (var key in keys)
                lastEvaluatedKey.Add(key, orderAttributeMap.GetValueOrDefault(key));

            return lastEvaluatedKey;
        }

        private Pagination GetPaginationFromQueryResponse(QueryResponse queryResponse)
        {
            string paginationToken = GetPaginationTokenFromQueryResponse(queryResponse);

            return new Pagination
            {
                PageSize = queryResponse.Count,
                PaginationToken = paginationToken
            };
        }

        /// <summary>
        /// GetPaginationTokenFromQueryResponse:
        /// - The QueryResponse.LastEvaluatedKey may not be the same as the key of the last item in the QueryResponse.Items
        ///   as the filter will have removed some of the items post query. Meaning if the table consists of:
        ///   ╔═════════╤═══════╤═══════╗
        ///   ║ OwnerId │ Name  │ Type  ║
        ///   ║ [PK]    │ [SK]  │       ║
        ///   ╠═════════╪═══════╪═══════╣
        ///   ║ 1       │ Item1 │ Type1 ║
        ///   ╟─────────┼───────┼───────╢
        ///   ║ 1       │ Item2 │ Type2 ║
        ///   ╟─────────┼───────┼───────╢
        ///   ║ 1       │ Item3 │ Type1 ║
        ///   ╟─────────┼───────┼───────╢
        ///   ║ 1       │ Item4 │ Type2 ║
        ///   ╚═════════╧═══════╧═══════╝
        ///   
        ///   Query (simplified):
        ///     {
        ///         Limit: 2,
        ///         KeyConditionExpression: "OwnerId = 1 AND begins_with(Name, Item)",
        ///         FilterExpression: "Type = Type1"
        ///     }
        ///   
        ///   The expected result will be just Item1, the QueryResponse.ScannedCount to be 2 
        ///   and the LastEvaluatedKey being the key of Item2. Why?
        ///   Because next time the same query is run, the query shouldn't query Item2 again, it should start at Item3
        /// </summary>
        /// <param name="queryResponse">The query response of a query with Limit set, aka. a pagination query</param>
        /// <returns>null or the pagination token of the LastEvaluatedKey</returns>
        private string GetPaginationTokenFromQueryResponse(QueryResponse queryResponse)
        {   
            if (queryResponse.LastEvaluatedKey.Count == 0)
                return null;
                
            var paginationToken = PaginationTokenConverter.GetToken<OrderEntity>(
                queryResponse.LastEvaluatedKey);

            return paginationToken;
        }

        /***
        * Query conditions
        ***/
        private List<string> KeyConditions { get; set; } = new List<string>();
        private List<string> FilterConditions { get; set; } = new List<string>();

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
                TableName = typeof(OrderEntity).GetDynamoDBTableName(),
                Select = "ALL_ATTRIBUTES",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {V_CREATED_FROM_DATE, ((DateTime) request.CreatedFromDate).GetAttributeValue()}
                },
                ScanIndexForward = false
            };

            var index = GetOptimalOrderIndex(request);
            query.IndexName = index;

            SetQueryConditions(request, query);
            SetQueryPagination(request, query);

            return query;
        }

        /// <summary>
        /// This function returns the index name of the index that will result in the least amount of records.
        /// As it's only possible to query one index at a time, it's important to choose the optimal one for the query.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The name of the optimal index for the given order</returns>
        public string GetOptimalOrderIndex(GetOrdersQuery request)
        {
            if (request.AssetSymbol != null)
                return OrderLocalSecondaryIndexes.UserOrderAssetIndex;

            if (request.PortfolioId != null)
                return OrderLocalSecondaryIndexes.UserOrderPortfolioIndex;

            if (request.Status != null)
                return OrderLocalSecondaryIndexes.UserOrderStatusIndex;

            if (request.Type != null)
                return OrderLocalSecondaryIndexes.UserOrderTypeIndex;

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
                    OrderLocalSecondaryIndexes.UserOrderTypeIndex,
                    query.IndexName,
                    $"begins_with({nameof(OrderEntity.OrderType)}, {V_TYPE})");

                query.ExpressionAttributeValues.Add(
                    V_TYPE,
                    request.Type.GetSelfContainingCompositeKeyQueryAttributeValue());
            }

            if (request.Status != null)
            {
                AddIndexCondition(
                    OrderLocalSecondaryIndexes.UserOrderStatusIndex,
                    query.IndexName,
                    $"begins_with({nameof(OrderEntity.OrderStatusCreatedAtCompositeKey)}, {V_STATUS})");

                query.ExpressionAttributeValues.Add(
                    V_STATUS,
                    new AttributeValue{ S = $"{Convert.ToUInt32(request.Status)}{MappingConstants.COMPOSITE_KEY_SEPARATOR}"});

            }

            if (request.PortfolioId != null)
            {
                AddIndexCondition(
                    OrderLocalSecondaryIndexes.UserOrderPortfolioIndex,
                    query.IndexName,
                    $"begins_with({nameof(OrderEntity.PortfolioId)}, {V_PORTFOLIO_ID})");

                query.ExpressionAttributeValues.Add(
                    V_PORTFOLIO_ID,
                    request.PortfolioId.GetSelfContainingCompositeKeyQueryAttributeValue());
            }

            if (request.AssetSymbol != null)
            {
                AddIndexCondition(
                    OrderLocalSecondaryIndexes.UserOrderAssetIndex,
                    query.IndexName,
                    $"begins_with({nameof(OrderEntity.AssetSymbol)}, {V_ASSET_SYMBOL})");

                query.ExpressionAttributeValues.Add(
                    V_ASSET_SYMBOL,
                    request.AssetSymbol.GetSelfContainingCompositeKeyQueryAttributeValue());
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

            query.KeyConditionExpression = string.Join(" AND ", KeyConditions);

            if (FilterConditions.Count > 0)
                query.FilterExpression = string.Join(" AND ", FilterConditions);
        }

        /// <summary>
        /// AddIndexCondition adds the condition to either the KeyConditions or the FilterConditions
        /// based on the selected index for the query.
        /// </summary>
        /// <param name="expectedIndex"></param>
        /// <param name="actualIndex"></param>
        /// <param name="condition"></param>
        private void AddIndexCondition(string expectedIndex, string actualIndex, string condition)
        {
            if (actualIndex == expectedIndex //If the indexes are null Equals will throw a NullReferenceException
                || actualIndex.Equals(expectedIndex))
                KeyConditions.Add(condition);
            else FilterConditions.Add(condition);
        }

        private void SetQueryPagination(GetOrdersQuery request, QueryRequest query)
        {
            query.Limit = request.PageSize;
            query.ExclusiveStartKey = GetLastEvaluatedKey(request, query);
        }

        private Dictionary<string, AttributeValue> GetLastEvaluatedKey(GetOrdersQuery request, QueryRequest query)
        {
            if (request.PaginationToken == null)
                return new Dictionary<string, AttributeValue>();
            
            if (query.IndexName != null)
                return PaginationTokenConverter.GetLastEvaluatedKeyFromTokenWithLocalSecondaryIndex<OrderEntity>(
                    request.PaginationToken,
                    query.IndexName);
            
            return PaginationTokenConverter.GetLastEvaluatedKeyFromToken<OrderEntity>(request.PaginationToken);
        }

        private async Task<List<OrderOverview>> SetOrdersJoinProperties(List<OrderOverview> orders)
        {
            foreach (var order in orders)
            {
                var portfolio = await _portfolioRepository.GetPortfolioById(order.PortfolioId);
                if (portfolio != null) order.PortfolioName = portfolio.Name;
            }

            return orders;
        }
    }
}