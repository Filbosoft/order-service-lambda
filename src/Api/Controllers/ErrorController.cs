using System;
using System.Diagnostics;
// using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        public readonly ILogger _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        public IActionResult Error()
        {
            var context = HttpContext;
            var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = errorFeature.Error;
            var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;

            _logger.LogError(exception, "Unhandled exception occurred. Trace Id: {traceId}", traceId);

            return Problem();
        }
    }
}