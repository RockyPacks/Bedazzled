using Bedazzled.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bedazzled.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class FirebaseAdminAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public const string SessionItemKey = "FirebaseAdminSession";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorizationHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Missing Firebase bearer token." });
            return;
        }

        var idToken = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(idToken))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Missing Firebase bearer token." });
            return;
        }

        var authService = context.HttpContext.RequestServices.GetRequiredService<IFirebaseAdminAuthService>();
        var session = await authService.ValidateAsync(idToken, context.HttpContext.RequestAborted);
        if (session is null)
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Invalid or expired Firebase session." });
            return;
        }

        context.HttpContext.Items[SessionItemKey] = session;
        await next();
    }
}
