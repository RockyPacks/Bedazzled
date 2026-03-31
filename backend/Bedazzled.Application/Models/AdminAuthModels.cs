namespace Bedazzled.Application.Models;

public class AdminLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminLoginResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}

public class AdminSessionResponse
{
    public string Email { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}
