﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
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

        // Get orders
        
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] string id)
        {
            throw new NotImplementedException();
        }

        // Update order

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get([FromQuery] List<string> type)
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
