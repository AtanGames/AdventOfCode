using System.Net;

namespace AdventOfCode;

public class FileFetch
{
    private string _uri;

    private static string? _sessionToken;
    
    public FileFetch(string uri)
    {
        _uri = uri;
    }
    
    public static void SetSessionToken(string? token)
    {
        _sessionToken = token;
    }
    
    public string FetchAsString()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        
        handler.CookieContainer.Add(new Uri(_uri), new Cookie("session", _sessionToken));
        
        using var client = new HttpClient(handler);
        
        var response = client.GetStringAsync(_uri).Result;
        return response;
    }
}