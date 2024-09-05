using System.Collections.Concurrent;

namespace DiscordMusicBot.Music
{
    public class GuildsVoiceClientManager
    {
        public ConcurrentDictionary<ulong, GuildVoiceClient> Clients { get; private set; }
        
        public GuildsVoiceClientManager() 
        {
            Clients = new ConcurrentDictionary<ulong, GuildVoiceClient>();
        }

        public GuildVoiceClient GetGuildVoiceClient(ulong guildId)
        {
            GuildVoiceClient client;
           
            if (!Clients.ContainsKey(guildId))
            {
                Clients.TryAdd(guildId, new GuildVoiceClient());
            }

            Clients.TryGetValue(guildId, out client);

            return client;
        }
    }
}
