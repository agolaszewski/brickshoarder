//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using NodaTime;
//using NodaTime.Serialization.SystemTextJson;
//using RealWorld.Backend.Configuration;
//using RealWorld.Backend.Filters;
//using RealWorld.Common;
//using RealWorld.Core.Services;
//using RealWorld.CurrentUser;
//using RealWorld.Database.Access;
//using RealWorld.Domain;
//using RealWorld.EventStore;
//using RealWorld.RabbitMq;
//using RealWorld.Redis;
//using RealWorld.Serilog;
//using RealWorld.Swagger;
//using Serilog;

//namespace RealWorld.Backend
//{
//    public class Startup
//    {
//        private readonly IConfigurationRoot _configuration;

//        private readonly SolutionConfiguration _solutionConfiguration;

//        public Startup(IWebHostEnvironment hostingEnvironment)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(hostingEnvironment.ContentRootPath)
//                .AddJsonFile("appsettings.json", false, true)
//                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true)
//                .AddEnvironmentVariables();

//            if (hostingEnvironment.IsDevelopment())
//            {
//                configuration.AddUserSecrets<Startup>(true);
//            }

//            _configuration = configuration.Build();
//            _solutionConfiguration = new SolutionConfiguration(_configuration);
//        }

//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddMvc().AddJsonOptions(o =>
//            {
//                o.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
//            });

//            services.AddControllers(configuration =>
//            {
//                configuration.Filters.Add<ApiExceptionAttribute>();
//            });

//            Log.Logger = Log.Logger.AddSerilog().AddSeq(_solutionConfiguration.SeqUrl).CreateLogger();

//            services.AddDatabase<AppDatabaseContext>(_solutionConfiguration.AppDatabaseCredentials.ConnectionString);

//            services.AddEventStore(_solutionConfiguration.EventStoreUrl);

//            services.AddRedis(_solutionConfiguration.RedisCredentials.ConnectionString);

//            services.AddRabbitMq<Test>(_solutionConfiguration.RabbitMqCredentials, _solutionConfiguration.RedisCredentials);

//            services.AddSwagger("RealWorld");

//            services.AddCurrentUserService();

//            services.AddSingleton<IClock>(b => SystemClock.Instance);

//            services.AddSingleton<IGuidService, GuidService>();
//        }

//        public void Configure(IApplicationBuilder app)
//        {
//            app.UseSerilogRequestLogging();

//            app.UseRouting();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });

//            app.UseSwagger();
//            app.UseSwaggerUI(c =>
//            {
//                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RealWorld.Api");
//                c.DisplayRequestDuration();
//            });
//        }
//    }
//}
