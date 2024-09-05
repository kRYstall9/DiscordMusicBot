using Discord;
using Discord.Audio;
using DiscordMusicBot.Interfaces;
using DiscordMusicBot.Music;
using DiscordMusicBot.Source;
using Serilog;
using static DiscordMusicBot.Utils.Utils;

namespace DiscordMusicBot.Services
{
    public class AudioService : IAudioService
    {
        public GuildsVoiceClientManager GuildVoiceClientManager {  get; private set; }

        public AudioService()
        {
            GuildVoiceClientManager = new GuildsVoiceClientManager();
        }
    
        public async Task StartTrackAsync(string query, IGuild guild, IMessageChannel textChannel)
        {
            bool isUrl = await IsUrl(query);

            List<AudioTrack> tracks = await ExtractTracksMetadata(query, isUrl: isUrl);

            if(tracks == null || !tracks.Any())
            {
                Log.Debug($"Song requested in {guild.Name} wasn't added to the queue");
                return;
            }

            try
            {
                if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
                {
                    client.AudioPlayer.TextChannel = textChannel;
                    await client.TrackManager.AddTracksToQueue(tracks);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.StackTrace);
            }
        }

        public async Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel textChannel)
        {
            IAudioClient audioClient = await channel.ConnectAsync();

            GuildVoiceClient guildVoiceClient = GuildVoiceClientManager.GetGuildVoiceClient(guild.Id);
            guildVoiceClient.AudioPlayer.SetAudioClient(audioClient, textChannel);

        }

        public async Task LeaveChannel(IVoiceChannel channel, IGuild guild)
        {
            if (GuildVoiceClientManager.Clients.TryRemove(guild.Id, out var client))
            {
                try
                {
                    client.AudioPlayer.Stop();
                    await client.AudioPlayer.AudioClient.StopAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                await channel.SendMessageAsync("I'm not connected");
            }
        }

        public async Task SkipTrackAsync(IVoiceChannel clientChannel, IGuild guild, int songsToSkip = 0)
        {
            if (GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                try
                {
                    await client.TrackManager.SkipCurrentTrack(songsToSkip);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Skipped");
                }
            }
        }
        
        public async Task StopVoiceActivity(IVoiceChannel clientChannel, IGuild guild)
        {
            if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                client.TrackManager.TrackQueue.Clear();
                client.AudioPlayer.Stop();
            }
        }
        
        public async Task PauseOrResume(IVoiceChannel clientChannel, IGuild guild, bool pause)
        {
            if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                client.AudioPlayer.Paused = pause;
            }
        } 
       
        public async Task<Embed> QueueEmbed(IGuild guild)
        {
            if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                List<AudioTrack> queueList = client.TrackManager.TrackQueue.ToList();
                return await FullQueueEmbed(queueList, "Queue");
            }
            return null;
        }
    }  
}
