using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Responses.V1;
using Business.Commands;
using Business.Queries;
using Conditus.Trader.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/[controller]")]
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

            switch (response.ResponseCode)
            {
                case CreateOrderResponseCodes.PortfolioNotFound:
                case CreateOrderResponseCodes.AssetNotFound:
                case CreateOrderResponseCodes.ValidationFailed:
                    var problem = new ProblemDetails
                    {
                        Title = response.ResponseCode.ToString(),
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(problem);

                case CreateOrderResponseCodes.Success:
                default:
                    var newOrder = response.Data;
                    var apiResponse = new ApiResponse<OrderDetail>
                    {
                        Data = newOrder,
                        Status = StatusCodes.Status201Created
                    };
                    return CreatedAtAction(
                        nameof(GetOrderById),
                        new { Id = newOrder.Id },
                        apiResponse
                    );
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] GetOrdersQuery query)
        {
            var response = await _mediator.Send(query);

            switch (response.ResponseCode)
            {
                case GetOrdersResponseCodes.Success:
                default:
                    var orders = response.Data;
                    var apiResponse = new PagedApiResponse<IEnumerable<OrderOverview>>
                    {
                        Data = orders,
                        Status = StatusCodes.Status200OK,
                        Pagination = response.Pagination
                    };
                    return Ok(apiResponse);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] string id)
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var response = await _mediator.Send(query);

            switch (response.ResponseCode)
            {
                case GetOrderByIdResponseCodes.OrderNotFound:
                    var problem = new ProblemDetails
                    {
                        Title = response.ResponseCode.ToString(),
                        Detail = response.Message,
                        Status = StatusCodes.Status404NotFound
                    };
                    return NotFound(problem);

                case GetOrderByIdResponseCodes.Success:
                default:
                    var order = response.Data;
                    var apiResponse = new ApiResponse<OrderDetail>
                    {
                        Data = order,
                        Status = StatusCodes.Status200OK
                    };
                    return Ok(apiResponse);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<OrderDetail>> UpdateOrder([FromRoute] string id, UpdateOrderCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);

            switch (response.ResponseCode)
            {
                case UpdateOrderResponseCodes.OrderNotFound:
                    var notFoundProblem = new ProblemDetails
                    {
                        Title = response.ResponseCode.ToString(),
                        Detail = response.Message,
                        Status = StatusCodes.Status404NotFound
                    };
                    return NotFound(notFoundProblem);

                case UpdateOrderResponseCodes.OrderNotActive:
                case UpdateOrderResponseCodes.ValidationFailed:
                    var badRequestProblem = new ProblemDetails
                    {
                        Title = response.ResponseCode.ToString(),
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(badRequestProblem);

                case UpdateOrderResponseCodes.NoUpdatesFound:
                case UpdateOrderResponseCodes.Success:
                default:
                    var updatedOrder = response.Data;
                    var apiResponse = new ApiResponse<OrderDetail>
                    {
                        Data = updatedOrder,
                        Status = StatusCodes.Status202Accepted
                    };
                    return Accepted(apiResponse);
            }
        }
    }
}
