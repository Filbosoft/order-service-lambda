﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Api.Responses.V1;
using Business;
using Business.Commands;
using Business.Queries;
using Conditus.Trader.Domain.Enums;
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

            switch (response.ResponseCode)
            {
                case CreateOrderResponseCodes.PortfolioNotFound:
                case CreateOrderResponseCodes.AssetNotFound:
                case CreateOrderResponseCodes.ValidationFailed:
                    var problem = new ProblemDetails
                    {
                        Title = response.ResponseCode.ToString(),
                        Detail = response.Message
                    };
                    return BadRequest(problem);

                case CreateOrderResponseCodes.Success:
                default:
                    var newOrder = response.Data;
                    return CreatedAtAction(
                        nameof(GetOrderById),
                        new { Id = newOrder.Id },
                        newOrder
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
                        StatusCode = StatusCodes.Status200OK,
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
                    return NotFound(response.Message);

                case GetOrderByIdResponseCodes.Success:
                default:
                    var order = response.Data;
                    return Ok(order);
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
                    return NotFound(response.Message);

                case UpdateOrderResponseCodes.OrderNotActive:
                case UpdateOrderResponseCodes.ValidationFailed:
                    return BadRequest(response.Message);

                case UpdateOrderResponseCodes.NoUpdatesFound:
                case UpdateOrderResponseCodes.Success:
                default:
                    var updatedOrder = response.Data;
                    return Accepted(updatedOrder);
            }
        }
    }
}
