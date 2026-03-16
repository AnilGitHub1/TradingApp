using TradingApp.Core.Entities;

namespace TradingApp.Core.DTOs
{
    public class SaveUserTrendlineDto
    {
        public int Token { get; set; }
        public string Tf { get; set; } = "";
        public decimal StartValue { get; set; }
        public long StartTime { get; set; }
        public decimal EndValue { get; set; }
        public long EndTime { get; set; }
        public decimal Slope { get; set; }
        public decimal Intercept { get; set; }
        public int? Index1 { get; set; }
        public int? Index2 { get; set; }
    }

    public class UpdateUserTrendlineDto
    {
        public string? Tf { get; set; }
        public decimal? StartValue { get; set; }
        public long? StartTime { get; set; }
        public decimal? EndValue { get; set; }
        public long? EndTime { get; set; }
        public decimal? Slope { get; set; }
        public decimal? Intercept { get; set; }
        public int? Index1 { get; set; }
        public int? Index2 { get; set; }
    }
    public class TrendlineResponseDto
    {
        public int Id { get; set; }
        public int Token { get; set; }
        public string Tf { get; set; }

        public decimal StartValue { get; set; }
        public long StartTime { get; set; }

        public decimal EndValue { get; set; }
        public long EndTime { get; set; }

        public decimal Slope { get; set; }
        public decimal Intercept { get; set; }

        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public static TrendlineResponseDto ToDto(UserTrendline t)
        {
            return new TrendlineResponseDto
            {
                Id = t.Id,
                Token = t.Token,
                Tf = t.Tf,
                StartValue = t.StartValue,
                StartTime = new DateTimeOffset(t.StartTime).ToUnixTimeSeconds(),
                EndValue = t.EndValue,
                EndTime = new DateTimeOffset(t.EndTime).ToUnixTimeSeconds(),
                Slope = t.Slope,
                Intercept = t.Intercept,
                Index1 = t.Index1 ?? 0,
                Index2 = t.Index2 ?? 1
            };
        }
    }
    
}
