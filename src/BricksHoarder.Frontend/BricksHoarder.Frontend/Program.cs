using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using BricksHoarder.Frontend.Services;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BricksHoarder.Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            var currentAssembly = typeof(Program).Assembly;
            builder.Services.AddFluxor(options =>
            {
                options.ScanAssemblies(currentAssembly);
                options.UseReduxDevTools(reduxDevToolsMiddlewareOptions =>
                {
                    reduxDevToolsMiddlewareOptions.UseNewtonsoftJson();
                });
            });

            builder.RootComponents.Add<App>("#app");
            builder.Services.AddServices();

            builder.Services.AddBlazoredLocalStorage();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}