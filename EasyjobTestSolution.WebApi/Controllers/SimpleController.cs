using EasyjobTestSolution.WebApi.Interfaces;
using EasyjobTestSolution.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace EasyjobTestSolution.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class SimpleController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _itemListEndpoint;
        private readonly ITokenService _tokenService;

        public SimpleController(HttpClient httpClient, IConfiguration configuration, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl not configured.");
            _itemListEndpoint = configuration["ApiSettings:Endpoints:ItemList"] ?? throw new ArgumentNullException("ItemList endpoint not configured.");
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
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

        private async Task<string> SendApiRequestAsync(TokenResponse token)
        {
            string apiUrl = $"{_baseUrl}{_itemListEndpoint}";
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
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
