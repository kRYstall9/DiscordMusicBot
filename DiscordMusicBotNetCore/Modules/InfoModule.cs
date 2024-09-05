using Discord;
using Discord.Commands;
using DiscordMusicBot.Attributes;

namespace DiscordMusicBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        
        public InfoModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("help")]
        [Summary("Provides the list of available commands\nType `!help <command>` to get more infos about the command")]
        [Alias("h")]
        [Usage("help <command>")]
        public async Task Help(string cmd = null)
        {
            List<ModuleInfo> moduleInfos = _commandService.Modules.ToList();
            
            bool noSpecifiedCommand = string.IsNullOrEmpty(cmd);
            
            EmbedBuilder embedBuilder = new EmbedBuilder();

            if (noSpecifiedCommand)
            {
                embedBuilder.Title= ":gear: Command List\n";
                embedBuilder.Color = Color.Purple;
                
                foreach(ModuleInfo module in moduleInfos)
                {
                    string moduleName = module.Name.Replace("Module", "");
                    string fieldName = string.Empty;

                    switch (moduleName.ToLower())
                    {
                        case "music":
                            fieldName = $":musical_score: {moduleName}";
                            break;
                        case "info":
                            fieldName = $":information_source: {moduleName}";
                            break;
                    }

                    List<string> commandsNames = module.Commands.Select(x => $"`{x.Name}`").ToList();

                    string commandsNamesString = String.Join(", ", commandsNames);

                    embedBuilder.AddField(fieldName, commandsNamesString);
                }

                embedBuilder.WithFooter(new EmbedFooterBuilder
                {
                    Text = "Type !help <command> to get specific command informations."
                });
            }
            else
            {
                CommandInfo requestedCommand = _commandService.Search(cmd).Commands.FirstOrDefault().Command;
                embedBuilder.Title = requestedCommand.Name;
                embedBuilder.Description = requestedCommand.Summary;

                string usageString = requestedCommand.Attributes.OfType<UsageAttribute>().FirstOrDefault().Usage;
                List<string> commandAliases = requestedCommand.Aliases.Where(x => String.Compare(x, requestedCommand.Name, true) != 0).Select(x => $"`{x}`").ToList();

                embedBuilder.AddField("Usage", usageString);
                string aliases = String.Join(",", commandAliases);

                if(!String.IsNullOrEmpty(aliases))
                    embedBuilder.AddField("Aliases", aliases);

                embedBuilder.Color = Color.Gold;
            }

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
