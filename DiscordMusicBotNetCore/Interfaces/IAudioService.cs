using Discord;

namespace DiscordMusicBot.Interfaces
{
    public interface IAudioService : IClientService
    {
        Task StartTrackAsync(string query, IGuild guild, IMessageChannel textChannel);
        Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel textChannel);
        Task LeaveChannel(IVoiceChannel channel, IGuild guild);
        Task SkipTrackAsync(IVoiceChannel clientChannel, IGuild guild, int songsToSkip = 0);
        Task StopVoiceActivity(IVoiceChannel clientChannel, IGuild guild);
        Task PauseOrResume(IVoiceChannel clientChannel, IGuild guild, bool pause);
        Task<Embed> QueueEmbed(IGuild guild);
    }
}
