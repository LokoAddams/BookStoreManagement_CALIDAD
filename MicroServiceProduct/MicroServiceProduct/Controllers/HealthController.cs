// HealthController: basic health check endpoint
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;

namespace MicroServiceProduct.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint. Returns basic status information.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult Get()
    {
        var info = new {
            status = "OK",
            timestamp = DateTime.UtcNow
        };
        return Ok(info);
    }
}
