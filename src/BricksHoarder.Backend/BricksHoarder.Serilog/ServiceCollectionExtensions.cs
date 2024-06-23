using Serilog;
using Serilog.Events;

namespace BricksHoarder.Serilog
{
    public static class ServiceCollectionExtensions
    {
        public static LoggerConfiguration AddSerilog(this ILogger logger)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Marten", LogEventLevel.Warning)
                .MinimumLevel.Override("ThrottlingTroll.ThrottlingTroll", LogEventLevel.Error)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Warning)
                .MinimumLevel.Override("Npgsql",LogEventLevel.Warning)
                .MinimumLevel.Override("Azure.Messaging.ServiceBus", LogEventLevel.Warning)
                .Enrich.FromLogContext();
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            //.MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
            //.MinimumLevel.Override("Marten", LogEventLevel.Warning)
            //.MinimumLevel.Override("ThrottlingTroll", LogEventLevel.Error)
            ////.WriteTo.Console(outputTemplate: format)
            //.Enrich.FromLogContext();
        }

        public static LoggerConfiguration AddConsole(this LoggerConfiguration loggerConfiguration)
        {
            const string format = "{Timestamp:yyyy} {SourceContext} [{Level:u3}] {NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";

            return loggerConfiguration.WriteTo.Console(outputTemplate: format, restrictedToMinimumLevel: LogEventLevel.Warning);
        }

        public static LoggerConfiguration AddSeq(this LoggerConfiguration loggerConfiguration, Uri seqUrl)
        {
            return loggerConfiguration.WriteTo.Seq(seqUrl.AbsoluteUri);
        }
    }
}