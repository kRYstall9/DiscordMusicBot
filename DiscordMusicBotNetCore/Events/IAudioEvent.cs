using Discord;
using Discord.Audio;
using DiscordMusicBot.Source;

namespace DiscordMusicBot.Events
{
    public interface IAudioEvent
    {
        delegate Task TrackStartAsync(AudioTrack audioTrack, IMessageChannel channel);
        event TrackStartAsync OnTrackStartAsync;

        delegate Task TrackEndAsync();
        event TrackEndAsync OnTrackEndAsync;

        delegate Task PlayerAFK(IAudioClient audioClient);
        event PlayerAFK OnPlayerAFK;
    }
}
