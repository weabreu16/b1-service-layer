#nullable disable
namespace B1ServiceLayer.Interfaces;

public interface ISAPConfig: ISAPCredentials
{
    string BaseUrl { get; set; }
}
