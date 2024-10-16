using EasyjobTestSolution.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EasyjobTestSolution.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class SimpleController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _username;
        private readonly string _password;

        public SimpleController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ApiSettings:Url"] ?? throw new ArgumentNullException("ApiSettings:Url not configured.");
            _username = configuration["ApiCredentials:Username"] ?? throw new ArgumentNullException("ApiCredentials:Username not configured.");
            _password = configuration["ApiCredentials:Password"] ?? throw new ArgumentNullException("ApiCredentials:Password not configured.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                var result = await SendApiRequestAsync(token);

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"External API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        private async Task<TokenResponse> GetTokenAsync()
        {
            string tokenUrl = $"{_apiUrl}/token";
            var tokenRequestContent = new StringContent($"grant_type=password&username={_username}&password={_password}");

            var tokenResponse = await _httpClient.PostAsync(tokenUrl, tokenRequestContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error when receiving a token: {tokenResponse.StatusCode}");
            }

            var responseContent = await tokenResponse.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<TokenResponse>(responseContent)
                        ?? throw new InvalidOperationException("Failed to deserialize token.");

            return token;
        }

        private async Task<string> SendApiRequestAsync(TokenResponse token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
            request.Headers.Add("ej-webapi-client", "ThirdParty");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API request failed: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
