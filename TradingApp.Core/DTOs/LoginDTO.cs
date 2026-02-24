namespace TradingApp.Core.DTOs
{
  public class LoginDto
  {
    public string email { get; set; }
    public string password { get; set; }
    public LoginDto(string email, string password)
    {
      this.email = email;
      this.password = password;
    }
  }
}
