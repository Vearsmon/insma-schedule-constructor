using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiController : ControllerBase
{
}