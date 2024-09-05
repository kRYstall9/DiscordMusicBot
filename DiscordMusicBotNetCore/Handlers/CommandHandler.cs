using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace DiscordMusicBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly char _commandPrefix = '!';

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider services )
        {
            _client = client;
            _commandService = commandService;
            _serviceProvider = services;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _serviceProvider);
        }

        //public async Task ExecuteAsync(SocketCommandContext context, string input)
        //{
        //    var result = await _commandService.ExecuteAsync(context, input, _serviceProvider);
        //    if (!result.IsSuccess)
        //    {
        //        var command = _commandService.Search(context, input).Commands.FirstOrDefault().Command;
        //        if(command != null)
        //        {

        //        }
        //    }
        //}

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {

            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix(_commandPrefix, ref argPos) 
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) 
                || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            await _commandService.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _serviceProvider);

        }
    }
}
