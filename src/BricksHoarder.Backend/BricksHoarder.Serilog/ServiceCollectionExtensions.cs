using Serilog;
using Serilog.Events;

namespace BricksHoarder.Serilog
{
    public static class ServiceCollectionExtensions
    {
        public static LoggerConfiguration AddSerilog(this ILogger logger)
        {
            const string format =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {SourceContext} [{Level:u3}] {NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Error)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Error)
                .WriteTo.Console(outputTemplate: format)
                .Enrich.FromLogContext();
        }

        public static LoggerConfiguration AddSeq(this LoggerConfiguration loggerConfiguration, Uri seqUrl)
        {
            return loggerConfiguration.WriteTo.Seq(seqUrl.AbsoluteUri);
        }
    }
}