using GvTelemetryProcessor.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using RexFunction;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RexFunction
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddGvApplicationInsightsTelemetryProcessor();
        }
    }
}
