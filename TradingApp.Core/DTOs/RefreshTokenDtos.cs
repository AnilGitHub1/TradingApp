namespace TradingApp.Core.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = "";
    }

    public class RevokeRefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = "";
    }
}
