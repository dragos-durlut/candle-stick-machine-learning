using CandleStickMachineLearning.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Threading.Tasks;

namespace CandleStickMachineLearning
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IYahooFinanceService, YahooFinanceService>()
                .AddHttpClient()
                .BuildServiceProvider();


            var loggerFactory = LoggerFactory.Create(builder => {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("CandleStickMachineLearning.Program", LogLevel.Debug)
                       .AddConsole();
            });

            var _logger = loggerFactory.CreateLogger<Program>();            

            _logger.LogInformation("Hello World!");

            //do the actual work here
            var yahooFinanceService = serviceProvider.GetService<IYahooFinanceService>();
            var barsList = await yahooFinanceService.GetBars("AAPL", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow, "1h");
        }
    }
}
