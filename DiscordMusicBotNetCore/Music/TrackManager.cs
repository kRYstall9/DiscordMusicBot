using Discord;
using Discord.Audio;
using DiscordMusicBot.Player;
using DiscordMusicBot.Source;
using DiscordMusicBotNetCore.CommandsModels;
using Serilog;
using static DiscordMusicBot.Utils.Utils;

namespace DiscordMusicBot.MusicManager
{
    public class TrackManager
    {
        public Queue<AudioTrack> TrackQueue { get; set; }
        private AudioPlayer _audioPlayer {  get; set; }

        public TrackManager(AudioPlayer audioPlayer)
        {
            TrackQueue = new Queue<AudioTrack>();
            _audioPlayer = audioPlayer;
            _audioPlayer.OnTrackStartAsync += OnTrackStartAsync;
            _audioPlayer.OnTrackEndAsync += OnTrackEndAsync;
            _audioPlayer.OnPlayerAFK += OnPlayerAFKAsync;
        }

        public async Task AddTracksToQueue(List<AudioTrack> tracks)
        {
            foreach (var track in tracks)
            {
                TrackQueue.Enqueue(track);
                Console.WriteLine($"{track.AudioTrackInfo.Title} added to the queue");
                //Log.Debug($"{track.AudioTrackInfo.Title} added to the queue");
            }

            if (tracks.Any())
            {
                if (tracks.Count > 1)
                {
                    await _audioPlayer.TextChannel.SendMessageAsync(embed: await FullQueueEmbed(tracks, "Queued songs"));
                }
                else
                {
                    AudioTrack track = tracks.FirstOrDefault();
                    string title = track.AudioTrackInfo.Title;
                    string thumbnailUrl = track.AudioTrackInfo?.ThumbnailUrl;

                    await _audioPlayer.TextChannel.SendMessageAsync(embed: await QueuedTrackEmbed(title, thumbnailUrl, TrackQueue.Count));
                }
            }

            if(_audioPlayer.PlayingTrack == null)
            {
                AudioTrack track = null;
                try
                {
                    track = TrackQueue.Dequeue();
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                await _audioPlayer.StartTrackAsync(track);
            }

            await Task.CompletedTask;
        }

        public async Task PlayNextTrack()
        {
            AudioTrack nextTrack;

            if (TrackQueue.Any())
            {
                nextTrack = TrackQueue.Dequeue();
                await _audioPlayer.StartTrackAsync(nextTrack);
            }
            else
            {
                await _audioPlayer.TextChannel.SendMessageAsync(":white_check_mark: **Queue ended**");
                _audioPlayer.Stop();
            }
            await Task.CompletedTask;
        }

        private async Task OnTrackStartAsync(AudioTrack audioTrack, IMessageChannel messageChannel)
        {
            string title = audioTrack.AudioTrackInfo.Title;
            string duration = $"`{await ConvertDuration(audioTrack.AudioTrackInfo.Duration)}`";
            string thumbnailUrl = audioTrack.AudioTrackInfo.ThumbnailUrl;

            await messageChannel.SendMessageAsync(embed: await CurrentTrackEmbed(title, duration, thumbnailUrl));
            await Task.CompletedTask;
        }

        private async Task OnTrackEndAsync()
        {
            await PlayNextTrack();
            await Task.CompletedTask;
        }

        public async Task SkipCurrentTrack(int songsToSkip = 0)
        {
            try
            {
                for (int i = 0; i < songsToSkip; i++)
                {
                    TrackQueue.Dequeue();
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
            }
            _audioPlayer.Stop();
            await Task.CompletedTask;
        }

        private async Task OnPlayerAFKAsync(IAudioClient audioClient)
        {
            if (!TrackQueue.Any())
            {
                Console.WriteLine("Kicking the bot");
                await audioClient.StopAsync();
                await _audioPlayer.TextChannel.SendMessageAsync(":warning: Disconnected due to inactivity");
            }

            await Task.CompletedTask;
        }
    
        public async Task MoveTrack(IMessageChannel messageChannel , MoveTrackModel moveTrackModel)
        {
            if(moveTrackModel.NewPos < 0 || moveTrackModel.NewPos > TrackQueue.Count)
            {
                await messageChannel.SendMessageAsync($"You can only choose indexes between 1 and {TrackQueue.Count}");
                return;
            }

            (Queue<AudioTrack>, string) result = await Utils.Utils.MoveTrack(TrackQueue, moveTrackModel.CurrentPos, moveTrackModel.NewPos);
            TrackQueue = result.Item1;

            await messageChannel.SendMessageAsync(result.Item2);
            await Task.CompletedTask;
        }
    
        public async Task RemoveBetween(IMessageChannel messageChannel, RemoveBetweenModel removeBetweenModel)
        {
            if (removeBetweenModel.EndPos <= removeBetweenModel.StartPos)
            {
                await messageChannel.SendMessageAsync(":warning: startPos cannot be greater/equal than endPos");
                return;
            }

            if(removeBetweenModel.StartPos < 0 || removeBetweenModel.EndPos > TrackQueue.Count)
            {
                await messageChannel.SendMessageAsync($":warning: startPos cannot be 0 and endPos cannot be greater than {TrackQueue.Count}");
                return;
            }

            (Queue<AudioTrack>, string) result = await Utils.Utils.RemoveBetween(TrackQueue, removeBetweenModel);
            
            TrackQueue = result.Item1;

            await messageChannel.SendMessageAsync(result.Item2);

            await Task.CompletedTask;
        }

        public async Task RemoveTrack(IMessageChannel messageChannel, int? trackPos)
        {
            if (!trackPos.HasValue)
            {
                await messageChannel.SendMessageAsync("You have to insert the track number to remove it from the queue");
            }
            else if (trackPos < 0 || trackPos > TrackQueue.Count)
            {
                await messageChannel.SendMessageAsync($"You can only remove a track between 1 and {TrackQueue.Count}");
            }
            else
            {
                TrackQueue = await Utils.Utils.RemoveTrack(TrackQueue, trackPos.Value);
                await messageChannel.SendMessageAsync(":white_check_mark: Track removed");
            }

            await Task.CompletedTask;
        }
    }

}
