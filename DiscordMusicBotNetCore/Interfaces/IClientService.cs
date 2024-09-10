using Discord;

namespace DiscordMusicBot.Interfaces
{
    public interface IClientService
    {
        Task JoinChannel(IVoiceChannel channel, IGuild guild, IMessageChannel textChannel);
        Task LeaveChannel(IMessageChannel channel, IGuild guild);
    }
}
