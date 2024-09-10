using Discord;
using Discord.Audio;
using DiscordMusicBot.Interfaces;
using DiscordMusicBot.Music;
using DiscordMusicBot.Source;
using DiscordMusicBotNetCore.CommandsModels;
using Serilog;
using System.Runtime.CompilerServices;
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

            await Task.CompletedTask;
        }

        public async Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel textChannel)
        {
            IAudioClient audioClient = await channel.ConnectAsync();

            GuildVoiceClient guildVoiceClient = GuildVoiceClientManager.GetGuildVoiceClient(guild.Id);
            guildVoiceClient.AudioPlayer.SetAudioClient(audioClient, textChannel);

            await Task.CompletedTask;

        }

        public async Task LeaveChannel(IMessageChannel channel, IGuild guild)
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

            await Task.CompletedTask;
        }

        public async Task SkipTrackAsync(IMessageChannel clientChannel, IGuild guild, int songsToSkip = 0)
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

            await Task.CompletedTask;
        }
        
        public async Task StopVoiceActivity(IMessageChannel clientChannel, IGuild guild)
        {
            if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                client.TrackManager.TrackQueue.Clear();
                client.AudioPlayer.Stop();
            }

            await Task.CompletedTask;
        }
        
        public async Task PauseOrResume(IMessageChannel clientChannel, IGuild guild, bool pause)
        {
            if(GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                client.AudioPlayer.Paused = pause;
            }
            await Task.CompletedTask;
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

        public async Task MoveTrack(IMessageChannel channel, IGuild guild, MoveTrackModel moveTrackModel)
        {

            if (!GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                await channel.SendMessageAsync("I'm not connected");
            }
            else 
            { 
                await client.TrackManager.MoveTrack(channel, moveTrackModel);
            }

            await Task.CompletedTask;
        }
    
        public async Task RemoveBetween(IMessageChannel messageChannel, IGuild guild, RemoveBetweenModel removeBetweenModel)
        {

            if(!GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                await messageChannel.SendMessageAsync("I'm not connected");
                return;
            }

            await client.TrackManager.RemoveBetween(messageChannel, removeBetweenModel);

        }
        
        public async Task RemoveTrack(IMessageChannel messageChannel, IGuild guild, int? trackPos)
        {
            if(!GuildVoiceClientManager.Clients.TryGetValue(guild.Id, out var client))
            {
                await messageChannel.SendMessageAsync("I'm not connected to any voice channel");
            }
            else
            {
                await client.TrackManager.RemoveTrack(messageChannel, trackPos);
            }

            await Task.CompletedTask;
        }
    }  
}
