namespace TradingApp.Core.DTOs
{
  public class RegisterDto
  {
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public RegisterDto(string name, string email, string password)
    {
      this.name = name;
      this.email = email;
      this.password = password;
    }
  }
}
