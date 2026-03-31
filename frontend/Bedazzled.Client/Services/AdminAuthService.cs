using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bedazzled.Application.Models;
using Microsoft.JSInterop;

namespace Bedazzled.Client.Services;

public sealed class AdminAuthService
{
    private const string AdminTokenStorageKey = "admin_id_token";

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public AdminAuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/auth/login", new AdminLoginRequest
            {
                Email = email.Trim(),
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorMessageAsync(response, "Unable to sign in.");
            }

            var payload = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
            if (payload is null || string.IsNullOrWhiteSpace(payload.IdToken))
            {
                return "Firebase returned an invalid login response.";
            }

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AdminTokenStorageKey, payload.IdToken);
            return null;
        }
        catch (Exception ex)
        {
            return $"Unable to reach the authentication server: {ex.Message}";
        }
    }

    public async Task<bool> HasValidSessionAsync()
    {
        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/auth/session");
        if (request is null)
        {
            return false;
        }

        using (request)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    await LogoutAsync();
                }
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public async Task<HttpRequestMessage?> CreateAuthorizedRequestAsync(HttpMethod method, string uri)
    {
        var idToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AdminTokenStorageKey);
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        return request;
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AdminTokenStorageKey);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return string.IsNullOrWhiteSpace(payload?.Message) ? fallbackMessage : payload.Message;
        }
        catch
        {
            return fallbackMessage;
        }
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
