using Bedazzled.Api.Infrastructure;
using Bedazzled.Api.Services;
using Bedazzled.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bedazzled.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IFirebaseAdminAuthService _firebaseAdminAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IFirebaseAdminAuthService firebaseAdminAuthService,
        ILogger<AuthController> logger)
    {
        _firebaseAdminAuthService = firebaseAdminAuthService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginResponse>> Login([FromBody] AdminLoginRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { Message = "Email and password are required." });
        }

        try
        {
            var response = await _firebaseAdminAuthService.LoginAsync(request, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(
                "Firebase login rejected for {Email}. Status {StatusCode}, code {FirebaseErrorCode}",
                request.Email,
                (int)ex.StatusCode,
                ex.FirebaseErrorCode ?? "n/a");

            return StatusCode((int)ex.StatusCode, new { Message = ex.UserMessage });
        }
    }

    [HttpGet("session")]
    [FirebaseAdminAuthorize]
    public ActionResult<AdminSessionResponse> GetSession()
    {
        var session = HttpContext.Items[FirebaseAdminAuthorizeAttribute.SessionItemKey] as ValidatedAdminSession;
        if (session is null)
        {
            return Unauthorized(new { Message = "Invalid or expired Firebase session." });
        }

        return Ok(new AdminSessionResponse
        {
            Email = session.Email,
            LocalId = session.LocalId,
            EmailVerified = session.EmailVerified
        });
    }
}
