using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMusicBot.Handlers;
using DiscordMusicBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using DiscordMusicBot.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace DiscordMusicBotNetCore
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private static IServiceProvider _serviceProvider;
        private static CommandService _commandService;
        private static CommandHandler _commandHandler;
        private DiscordSocketClient _client;
        private IConfiguration _configuration;

        private Program()
        {
            string logPath = Path.Combine(@"logs.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Infinite, fileSizeLimitBytes: null)
                .CreateLogger();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.All
            });

            _commandService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false
            });

            _client.Log += LogAsync;

            _commandService.Log += LogAsync;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddSingleton(_configuration)
                .AddSingleton<IAudioService, AudioService>()
                .BuildServiceProvider();
        }

        private static async Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
            };

            Log.Write(severity, message.Exception, $"[{message.Source}] {message.Message}");
            await Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            _configuration = BuildConfig();

            _serviceProvider = ConfigureServices();

            _commandHandler = new CommandHandler(_client, _commandService, _serviceProvider);

            await _commandHandler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, _configuration.GetSection("token").Value);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("appsettings.json")
                .Build();
        }

    }
}
