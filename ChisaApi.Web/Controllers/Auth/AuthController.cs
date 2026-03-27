using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChisaApi.Application.Auth;
using ChisaApi.Application.Auth.DataTransfer.Requests;
using ChisaApi.Application.Auth.DataTransfer.Responses;

namespace ChisaApi.Web.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _auth;

    public AuthController(AuthAppService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            TokenResponseDto result = await _auth.RegisterAsync(dto, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            TokenResponseDto result = await _auth.LoginAsync(dto, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
