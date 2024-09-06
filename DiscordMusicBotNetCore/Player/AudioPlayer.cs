using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using DiscordMusicBot.Events;
using DiscordMusicBot.Source;
using System.Timers;

namespace DiscordMusicBot.Player
{
    public class AudioPlayer : IAudioEvent
    {
        private volatile bool _paused = false;
        public AudioTrack PlayingTrack { get; set; }
        public Stream DiscordStream { get; set; }
        public IAudioClient AudioClient { get; set; }
        public IMessageChannel TextChannel { get; set; }
        private DateTime _timeLastSong {  get; set; }
        private System.Timers.Timer _timer;

        public event IAudioEvent.TrackStartAsync OnTrackStartAsync;
        public event IAudioEvent.TrackEndAsync OnTrackEndAsync;
        public event IAudioEvent.PlayerAFK OnPlayerAFK;

        private CancellationTokenSource _cancellationTokenSource;

        public AudioPlayer()
        {
            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnPlayerAFK += PlayerAFK;
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        public AudioPlayer(IAudioClient audioClient)
        {
            this.AudioClient = audioClient;

            DiscordStream = this.AudioClient.CreatePCMStream(AudioApplication.Mixed, bitrate: 128000, bufferMillis: 200);

            #region Events

            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;

            #endregion

        }

        public AudioPlayer(IAudioClient audioClient, IMessageChannel channel)
        {
            this.AudioClient = audioClient;
            DiscordStream = this.AudioClient.CreatePCMStream(AudioApplication.Mixed, bitrate: 128000, bufferMillis: 200);  
            
            #region Events

            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;

            #endregion

            TextChannel = channel;
        }

        public void SetAudioClient(IAudioClient audioClient, IMessageChannel channel)
        {
            this.AudioClient = audioClient;
            DiscordStream?.Dispose();
            DiscordStream = this.AudioClient.CreatePCMStream(AudioApplication.Mixed, bitrate: 128000, bufferMillis: 200);
            TextChannel = channel;
        }

        public async Task StartTrackAsync(AudioTrack audioTrack)
        {

            if(audioTrack == null)
            {
                Console.WriteLine("No track provided");
                return;
            }

            if(PlayingTrack != null)
            {
                Stop();
            }

            this.PlayingTrack = audioTrack;

            Console.WriteLine($"Playing {PlayingTrack.AudioTrackInfo.Title}");

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            await OnTrackStartAsync(PlayingTrack, TextChannel).ConfigureAwait(false);
            await TrackLoopAsync(PlayingTrack, _cancellationTokenSource.Token).ConfigureAwait(false);
            await OnTrackEndAsync().ConfigureAwait(false);
        }

        private Task TrackStartEventAsync (AudioTrack audioTrack, IMessageChannel textChannel)
        {
            _paused = false;
            PlayingTrack.LoadProcess();
            _timer.Stop();

            return Task.CompletedTask;
        }
        
        private Task TrackEndEventAsync()
        {
            _paused = false;
            ResetStream();
            PlayingTrack = null;
            _cancellationTokenSource?.Dispose();
            _timeLastSong = DateTime.Now;
            _timer.Start();

            return Task.CompletedTask;

        }

        private async Task TrackLoopAsync(AudioTrack audioTrack, CancellationToken cancellationToken)
        {
            int read = -1;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (DiscordStream == null)
                    {
                        Console.Error.WriteLine("Error when playing the audio track: Discord stream gone");
                        return;
                    }

                    if (!_paused)
                    {
                        read = await audioTrack.AudioTrackStream.ReadAsync(audioTrack.BufferFrame, 0, audioTrack.BufferFrame.Length, cancellationToken).ConfigureAwait(false);
                        

                        if (read > 0)
                        {
                            await DiscordStream.WriteAsync(audioTrack.BufferFrame, 0, read, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                PlayingTrack?.Dispose();
                PlayingTrack = null;
                Console.Error.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException)
            {
                PlayingTrack?.Dispose();
                PlayingTrack = null;
            }
            
        }

        private async Task PlayerAFK(IAudioClient audioClient)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("OnTimedEvent");

            if(AudioClient.ConnectionState == ConnectionState.Disconnected)
            {
                _timer.Stop();
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(_timeLastSong);
            if((PlayingTrack == null) && (timeSpan.TotalMinutes > 2))
            {
                Console.WriteLine("Stopping the timer");
                await OnPlayerAFK(AudioClient);
            }
            
        }

        public void Stop()
        {
            try
            {
                if(_cancellationTokenSource != null)
                    _cancellationTokenSource.Cancel(false);
            }
            catch (ObjectDisposedException) { }
        }

        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        protected void ResetStream()
        {
            DiscordStream?.Flush();
            PlayingTrack?.Dispose();
        }

        ~AudioPlayer()
        {
            DiscordStream?.Dispose();
            PlayingTrack?.Dispose();
            _cancellationTokenSource?.Dispose();
            _timer?.Dispose();
        }
    }
}
