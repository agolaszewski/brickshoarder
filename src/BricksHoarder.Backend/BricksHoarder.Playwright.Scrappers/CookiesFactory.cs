using System.Text.Json;
using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public class CookiesFactory
    {
        public async Task<IReadOnlyList<Cookie>> CreateCookiesAsync(string page)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Cookies/{page}.json");
            var content = await File.ReadAllTextAsync(path);

            var cookies = JsonSerializer.Deserialize<List<Cookie>>(content)!.ToList();
            return cookies;
        }
    }
}