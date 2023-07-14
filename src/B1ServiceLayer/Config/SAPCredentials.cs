#nullable disable
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer.Config;

public class SAPCredentials : ISAPCredentials
{
    public string CompanyDB { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
