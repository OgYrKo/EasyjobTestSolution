using EasyjobTestSolution.WebApi.Interfaces;
using EasyjobTestSolution.WebApi.Models;
using System.Text.Json;

namespace EasyjobTestSolution.WebApi.Services
{
    public sealed class TokenService: ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly string _tokenUrl;
        private readonly string _username;
        private readonly string _password;

        private TokenResponse _token;

        // Semaphore for thread-safe token requests
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        public TokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            //set the token url
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new ArgumentNullException("ApiSettings:BaseUrl not configured.");
            var tokenEndpoint = configuration["ApiSettings:Endpoints:Token"] ?? throw new ArgumentNullException("ApiSettings:Endpoints:Token not configured.");
            _tokenUrl = $"{baseUrl}{tokenEndpoint}";

            //set credentials
            _username = configuration["ApiCredentials:Username"] ?? throw new ArgumentNullException("ApiCredentials:Username not configured.");
            _password = configuration["ApiCredentials:Password"] ?? throw new ArgumentNullException("ApiCredentials:Password not configured.");
            _token = new TokenResponse();
        }

        public async Task<TokenResponse> GetTokenAsync()
        {
            if (_token.IsValid())
            {
                return _token; 
            }

            // Wait to enter the semaphore if another request is already obtaining a token
            await _tokenSemaphore.WaitAsync();

            try
            {
                // Double-check token validity after acquiring the semaphore
                if (_token.IsValid())
                {
                    return _token;
                }

                // If we are here, the token has expired or hasn't been requested yet.
                await UpdateTokenAsync();

                return _token;
            }
            finally
            {
                // Release the semaphore so other threads can acquire it
                _tokenSemaphore.Release();
            }
        }
        private async Task UpdateTokenAsync()
        {
            var tokenRequestContent = new StringContent($"grant_type=password&username={_username}&password={_password}");

            var tokenResponse = await _httpClient.PostAsync(_tokenUrl, tokenRequestContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error when receiving a token: {tokenResponse.StatusCode}");
            }

            var responseContent = await tokenResponse.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<TokenResponse>(responseContent)
                        ?? throw new InvalidOperationException("Failed to deserialize token.");

            _token = token;
        }
    }

}
