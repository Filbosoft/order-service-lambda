using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}")]
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

        // Get orders
        
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] string id)
        {
            throw new NotImplementedException();
        }

        // Update order
    }
}
