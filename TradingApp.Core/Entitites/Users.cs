namespace TradingApp.Core.Entities
{
  public class Users
  {    
    public int id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }

    public Users(string name, string email, string password)
    {
      this.name = name;
      this.email = email;
      this.password = password;
    }
    public static Users EmptyUser()
    {
      return new Users("", "","");
    }
  }
}
