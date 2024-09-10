using Discord;
using DiscordMusicBotNetCore.CommandsModels;

namespace DiscordMusicBot.Interfaces
{
    public interface IAudioService : IClientService
    {
        Task StartTrackAsync(string query, IGuild guild, IMessageChannel messageChannel);
        Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel messageChannel);
        Task LeaveChannel(IMessageChannel messageChannel, IGuild guild);
        Task SkipTrackAsync(IMessageChannel messageChannel, IGuild guild, int songsToSkip = 0);
        Task StopVoiceActivity(IMessageChannel messageChannel, IGuild guild);
        Task PauseOrResume(IMessageChannel messageChannel, IGuild guild, bool pause);
        Task MoveTrack(IMessageChannel messageChannel, IGuild guild, MoveTrackModel moveTrackModel);
        Task RemoveBetween(IMessageChannel messageChannel, IGuild guild, RemoveBetweenModel removeBetweenModel);
        Task RemoveTrack(IMessageChannel messageChannel, IGuild guild, int? trackPos);
        Task<Embed> QueueEmbed(IGuild guild);
    }
}
