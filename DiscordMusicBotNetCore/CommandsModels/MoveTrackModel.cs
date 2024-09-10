using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordMusicBotNetCore.CommandsModels
{
    [NamedArgumentType]
    public class MoveTrackModel
    {
        public int CurrentPos { get; set; } = 0;
        public int NewPos { get; set; } = 0;
    }
}
