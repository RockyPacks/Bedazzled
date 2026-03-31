using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Bedazzled.Application.Models;

namespace Bedazzled.Api.Services;

public interface IFirebaseAdminAuthService
{
    Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default);
    Task<ValidatedAdminSession?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class ValidatedAdminSession
{
    public string Email { get; init; } = string.Empty;
    public string LocalId { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
}

public sealed class FirebaseAuthException : Exception
{
    public FirebaseAuthException(HttpStatusCode statusCode, string userMessage, string? firebaseErrorCode = null)
        : base(userMessage)
    {
        StatusCode = statusCode;
        UserMessage = userMessage;
        FirebaseErrorCode = firebaseErrorCode;
    }

    public HttpStatusCode StatusCode { get; }
    public string UserMessage { get; }
    public string? FirebaseErrorCode { get; }
}

public sealed class FirebaseAdminAuthService : IFirebaseAdminAuthService
{
    private const string FirebaseRestBaseUrl = "https://identitytoolkit.googleapis.com/v1/accounts:";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebaseAdminAuthService> _logger;

    public FirebaseAdminAuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FirebaseAdminAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new FirebaseAuthException(HttpStatusCode.BadRequest, "Email and password are required.");
        }

        var response = await _httpClient.PostAsJsonAsync(
            BuildEndpoint("signInWithPassword"),
            new FirebaseSignInRequest
            {
                Email = request.Email.Trim(),
                Password = request.Password,
                ReturnSecureToken = true
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateFirebaseAuthExceptionAsync(response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>(cancellationToken: cancellationToken)
            ?? throw new FirebaseAuthException(HttpStatusCode.BadGateway, "Firebase returned an empty authentication response.");

        EnsureAdminAccess(payload.Email);

        return new AdminLoginResponse
        {
            IdToken = payload.IdToken,
            RefreshToken = payload.RefreshToken,
            Email = payload.Email,
            LocalId = payload.LocalId,
            ExpiresInSeconds = int.TryParse(payload.ExpiresIn, out var expiresInSeconds) ? expiresInSeconds : 0
        };
    }

    public async Task<ValidatedAdminSession?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var response = await _httpClient.PostAsJsonAsync(
            BuildEndpoint("lookup"),
            new FirebaseLookupRequest { IdToken = idToken },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var authException = await TryCreateFirebaseAuthExceptionAsync(response, cancellationToken);
            _logger.LogInformation(
                "Firebase session validation failed with status {StatusCode} and code {FirebaseErrorCode}",
                (int)response.StatusCode,
                authException?.FirebaseErrorCode ?? "unknown");

            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<FirebaseLookupResponse>(cancellationToken: cancellationToken);
        var user = payload?.Users?.FirstOrDefault();
        if (user is null)
        {
            return null;
        }

        if (!IsAllowedAdminEmail(user.Email))
        {
            _logger.LogWarning("Rejected Firebase-authenticated user {Email} because the account is not in the admin allowlist.", user.Email);
            return null;
        }

        return new ValidatedAdminSession
        {
            Email = user.Email,
            LocalId = user.LocalId,
            EmailVerified = user.EmailVerified
        };
    }

    private string BuildEndpoint(string action)
    {
        var webApiKey = _configuration["Firebase:WebApiKey"];
        if (string.IsNullOrWhiteSpace(webApiKey))
        {
            throw new FirebaseAuthException(
                HttpStatusCode.InternalServerError,
                "Firebase Web API key is missing. Configure Firebase:WebApiKey before using admin authentication.");
        }

        return $"{FirebaseRestBaseUrl}{action}?key={Uri.EscapeDataString(webApiKey)}";
    }

    private void EnsureAdminAccess(string email)
    {
        if (!IsAllowedAdminEmail(email))
        {
            throw new FirebaseAuthException(
                HttpStatusCode.Forbidden,
                "This Firebase account is not authorized to access the admin area.");
        }
    }

    private bool IsAllowedAdminEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var allowedEmails = _configuration
            .GetSection("Firebase:AllowedAdminEmails")
            .Get<string[]>();

        if (allowedEmails is null || allowedEmails.Length == 0)
        {
            return false;
        }

        return allowedEmails.Any(allowedEmail =>
            string.Equals(allowedEmail?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<FirebaseAuthException> CreateFirebaseAuthExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await TryCreateFirebaseAuthExceptionAsync(response, cancellationToken)
            ?? new FirebaseAuthException(response.StatusCode, "Firebase authentication failed.");
    }

    private static async Task<FirebaseAuthException?> TryCreateFirebaseAuthExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>(cancellationToken: cancellationToken);
            var errorCode = payload?.Error?.Message;
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                return null;
            }

            return new FirebaseAuthException(response.StatusCode, MapFirebaseErrorMessage(errorCode), errorCode);
        }
        catch
        {
            return null;
        }
    }

    private static string MapFirebaseErrorMessage(string errorCode)
    {
        return errorCode switch
        {
            "EMAIL_NOT_FOUND" => "Invalid email or password.",
            "INVALID_PASSWORD" => "Invalid email or password.",
            "INVALID_LOGIN_CREDENTIALS" => "Invalid email or password.",
            "USER_DISABLED" => "This Firebase account has been disabled.",
            "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many sign-in attempts. Try again later.",
            "TOKEN_EXPIRED" => "Your session has expired. Please sign in again.",
            "INVALID_ID_TOKEN" => "Your session is no longer valid. Please sign in again.",
            "API_KEY_SERVICE_BLOCKED" => "The configured Firebase API key cannot call Authentication endpoints.",
            _ => "Firebase authentication failed."
        };
    }

    private sealed class FirebaseSignInRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("returnSecureToken")]
        public bool ReturnSecureToken { get; set; }
    }

    private sealed class FirebaseSignInResponse
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("localId")]
        public string LocalId { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")]
        public string ExpiresIn { get; set; } = string.Empty;
    }

    private sealed class FirebaseLookupRequest
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; } = string.Empty;
    }

    private sealed class FirebaseLookupResponse
    {
        [JsonPropertyName("users")]
        public List<FirebaseLookupUser>? Users { get; set; }
    }

    private sealed class FirebaseLookupUser
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("localId")]
        public string LocalId { get; set; } = string.Empty;

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
    }

    private sealed class FirebaseErrorResponse
    {
        [JsonPropertyName("error")]
        public FirebaseErrorBody? Error { get; set; }
    }

    private sealed class FirebaseErrorBody
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
