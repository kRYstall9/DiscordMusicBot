using Discord;

namespace DiscordMusicBot.Interfaces
{
    public interface IClientService
    {
        Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel textChannel);
        Task LeaveChannel(IVoiceChannel channel, IGuild guild);
    }
}
