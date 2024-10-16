using EasyjobTestSolution.WebApi.Models;

namespace EasyjobTestSolution.WebApi.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GetTokenAsync();
    }
}
