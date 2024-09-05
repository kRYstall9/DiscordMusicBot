using Discord;
using Discord.Audio;
using DiscordMusicBot.Player;
using DiscordMusicBot.Source;
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
        }

        private async Task OnTrackStartAsync(AudioTrack audioTrack, IMessageChannel textChannel)
        {
            string title = audioTrack.AudioTrackInfo.Title;
            string duration = $"`{await ConvertDuration(audioTrack.AudioTrackInfo.Duration)}`";
            string thumbnailUrl = audioTrack.AudioTrackInfo.ThumbnailUrl;

            await textChannel.SendMessageAsync(embed: await CurrentTrackEmbed(title, duration, thumbnailUrl));
        }

        private async Task OnTrackEndAsync()
        {
            await PlayNextTrack();
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
    }
}
