﻿using System.Text.Json;
using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public class CookiesFactory
    {
        public async Task<IReadOnlyList<Cookie>> CreateCookiesAsync(string page)
        {
            var content = await File.ReadAllTextAsync($"Cookies/{page}.json");
            var cookies = JsonSerializer.Deserialize<List<Cookie>>(content)!.ToList();
            return cookies;
        }
    }
}