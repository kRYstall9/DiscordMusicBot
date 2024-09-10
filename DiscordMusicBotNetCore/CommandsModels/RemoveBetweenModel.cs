using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordMusicBotNetCore.CommandsModels
{
    [NamedArgumentType]
    public class RemoveBetweenModel
    {
        public int StartPos { get; set; } = 0;
        public int EndPos { get; set; } = 0;
        public bool Include { get; set; } = false;
    } 
}
