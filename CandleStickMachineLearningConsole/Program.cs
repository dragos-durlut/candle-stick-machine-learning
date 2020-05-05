using CandleStickMachineLearning.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
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

            var context = new MLContext();

            var trainData = context.Data.LoadFromEnumerable<Models.Bar>(barsList);

            var settings = new RegressionExperimentSettings
            {
                MaxExperimentTimeInSeconds = 20,
                OptimizingMetric = RegressionMetric.MeanAbsoluteError
            };

            var labelColumnInfo = new ColumnInformation()
            {
                LabelColumnName = "Label"
            };

            var progress = new Progress<RunDetail<RegressionMetrics>>(p =>
            {
                if (p.ValidationMetrics != null)
                {
                    _logger.LogInformation($"Current Result - {p.TrainerName}, {p.ValidationMetrics.RSquared}, {p.ValidationMetrics.MeanAbsoluteError}");
                }
            });

            var experiment = context.Auto().CreateRegressionExperiment(settings);

            var result = experiment.Execute(trainData, labelColumnInfo, progressHandler: progress);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Best run:");
            Console.WriteLine($"Trainer name - {result.BestRun.TrainerName}");
            Console.WriteLine($"RSquared - {result.BestRun.ValidationMetrics.RSquared}");
            Console.WriteLine($"MAE - {result.BestRun.ValidationMetrics.MeanAbsoluteError}");

            Console.ReadLine();
        }
    }
}
