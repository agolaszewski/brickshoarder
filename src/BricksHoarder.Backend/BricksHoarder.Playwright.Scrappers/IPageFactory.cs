using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public interface IPageFactory
    {
        Task<IPage> CreatePageAsync();
    }
}