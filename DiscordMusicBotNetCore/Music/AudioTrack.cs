using Discord.Audio;
using DiscordMusicBot.Common;
using System.Diagnostics;

namespace DiscordMusicBot.Source
{
    public class AudioTrack : IDisposable
    {
        private bool _disposed;

        public byte[] BufferFrame = new byte[1024];
        public AudioTrackInfo AudioTrackInfo { get; set; }
        public Stream AudioTrackStream { get; set; }
        public Process FFmpegProcess { get; set; }

        public void LoadProcess()
        {
            string filename = $"cmd.exe";

            string arguments = $"/c yt-dlp -x --audio-format best --audio-quality 0 -o - {this.AudioTrackInfo.Url} | ffmpeg -hide_banner -loglevel warning -i pipe:0 -ac 2 -f s16le -ar 48000 -b:a 128k pipe:1";

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            FFmpegProcess = Process.Start(startInfo);

            AudioTrackStream = FFmpegProcess.StandardOutput.BaseStream;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //TODO
                }

                AudioTrackStream?.Dispose();
                FFmpegProcess?.Dispose();
                AudioTrackStream = null;
                FFmpegProcess = null;
                _disposed = true;
            }
        }

        ~AudioTrack()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
