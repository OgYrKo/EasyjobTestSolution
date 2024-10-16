using System.Globalization;
using System.Text.Json.Serialization;

namespace EasyjobTestSolution.WebApi.Models
{
    public sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }= string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName(".issued")]
        public string IssuedString 
        {
            get
            {
                return Issued.ToString(DateFormat);
            }
            set
            {
                Issued = DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
            }
        } 

        public DateTime Issued { get; set; }

        [JsonPropertyName(".expires")]
        public string ExpiresString
        {
            get
            {
                return Expires.ToString(DateFormat);
            }
            set
            {
                Expires = DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
            }
        }
        public DateTime Expires { get; set; }

        private const string DateFormat = "ddd, dd MMM yyyy HH:mm:ss 'GMT'";

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AccessToken) && DateTime.UtcNow < Expires;
        }
    }
}
