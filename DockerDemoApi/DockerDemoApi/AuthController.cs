using Microsoft.AspNetCore.Mvc;

using DockerDemoApi.Domain;

namespace DockerDemoApi
{
	[Route("api/v{version:apiVersion}/auth")]
	[ApiVersion("1.0")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		[HttpPost]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(500)]
		public IActionResult Auth([FromBody] AuthRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			return Ok("success");
		}
	}
}
