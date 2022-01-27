using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MongoDbLogging
{
    public class Program : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger<Program> _logger;
        private int _exitCode;


        private static async Task<int> Main(string[] args)
        {
            try
            {
#pragma warning disable CA1416
                await Host.CreateDefaultBuilder(args)
                    .UseSerilog((context, config) => { config.ReadFrom.Configuration(context.Configuration); })
                    .ConfigureServices((hostContext, services) => services.AddHostedService<Program>())
                    //.UseConsoleLifetime(o => o.SuppressStatusMessages = true)
#pragma warning restore CA1416
                    .RunConsoleAsync(o => o.SuppressStatusMessages = true);

                return 0;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
        }

        public Program(IConfiguration configuration, IHostApplicationLifetime appLifetime, ILogger<Program> logger)
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _configuration = configuration;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Info");
                _logger.LogDebug("Debug");
                _logger.LogError("Error");
            }
            catch (Exception e)
            {
                WriteMessage("Error", e.Message, ConsoleColor.Red);
                _exitCode = -1;
            }
            finally
            {
                _appLifetime.StopApplication();
            }


            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = _exitCode;
            return Task.CompletedTask;
        }

        private static void WriteMessage(string header, string message, ConsoleColor headerColor)
        {
            var currentColor = Console.ForegroundColor;

            Console.ForegroundColor = headerColor;

            try
            {
                Console.Write($"{header}: ");
            }
            finally
            {
                Console.ForegroundColor = currentColor;
            }

            Console.WriteLine(message);
            Console.WriteLine();
        }
    }
}