using DiscordMusicBot.MusicManager;
using DiscordMusicBot.Player;

namespace DiscordMusicBot.Music
{
    public class GuildVoiceClient
    {
        public AudioPlayer AudioPlayer;
        public TrackManager TrackManager;

        public GuildVoiceClient()
        {
            AudioPlayer = new AudioPlayer();
            TrackManager = new TrackManager(AudioPlayer);
        }
    }
}
