using System;
using System.Threading.Tasks;
using Business.Commands;
using Business.Queries;
using Conditus.Trader.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;        
        
        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var response = await _mediator.Send(command);

            if (response.IsError)
                return BadRequest(response.Message);

            var newOrder = response.Data;
            return CreatedAtAction(
                nameof(GetOrderById),
                new { Id = newOrder.Id },
                newOrder
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string portfolioId,
            [FromQuery] OrderType? type,
            [FromQuery] string assetSymbol,
            [FromQuery] AssetType? assetType,
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? createdFromDate,
            [FromQuery] DateTime? createdToDate,
            [FromQuery] DateTime? completedFromDate,
            [FromQuery] DateTime? completedToDate)
        {
            var query = new GetOrdersQuery
            {
                PortfolioId = portfolioId,
                Type = type,
                AssetSymbol = assetSymbol,
                AssetType = assetType,
                Status = status,
                CreatedFromDate = createdFromDate,
                CreatedToDate = createdToDate,
                CompletedFromDate = completedFromDate,
                CompletedToDate = completedToDate
            };
            var response = await _mediator.Send(query);

            if (response.IsError)
                return NotFound(response.Message);

            var orders = response.Data;
            return Ok(orders);
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] string id)
        {
            var query = new GetOrderByIdQuery {OrderId = id};
            var response = await _mediator.Send(query);

            if (response.IsError)
                return NotFound(response.Message);

            var order = response.Data;
            return Ok(order);
        }
    }
}
