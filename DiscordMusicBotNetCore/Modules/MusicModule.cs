using Discord;
using Discord.Commands;
using DiscordMusicBot.Attributes;
using DiscordMusicBot.Interfaces;
using DiscordMusicBotNetCore.CommandsModels;

namespace DiscordMusicBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {

        private readonly IAudioService _audioService;
        private readonly string _emoji = ":musical_score:";

        public MusicModule(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins your voice channel")]
        [Usage("join")]
        [Alias("j")]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser).VoiceChannel;

            if (channel == null) { await Context.Channel.SendMessageAsync("You're not in a voice channel"); return; }

            var guild = channel.Guild;

            await _audioService.JoinChannel(channel, guild, Context.Channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Starts streaming the requested song.")]
        [Usage("play <youtube-url or song name>")]
        [Alias("p")]
        public async Task PlayAudio([Remainder] string query)
        {
            var userChannel = (Context.User as IGuildUser).VoiceChannel;

            if (userChannel == null) { await ReplyAsync("You're not in a voice channel"); return; }

            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (clientVoiceChannel == null || (String.Compare(clientVoiceChannel.Name, userChannel.Name) != 0))
            {
                await _audioService.JoinChannel(userChannel, guild, Context.Channel);
            }

            await _audioService.StartTrackAsync(query, guild, Context.Channel);
        }

        [Command("disconnect", RunMode = RunMode.Async)]
        [Summary("Disconnects from the voice channel")]
        [Usage("disconnect")]
        [Alias("leave")]
        public async Task LeaveChannel()
        {
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (clientVoiceChannel == null) { return; }

            await _audioService.LeaveChannel(Context.Channel, guild);

            Console.WriteLine($"Disconnected from {guild.Name}");
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Jumps to the next queued track")]
        [Usage("skip <number of songs to skip>.\n-# Default is 0 since it will skip the currently playing song")]
        public async Task SkipTrack(int songsToSkip = 0)
        {
            var userChannel = (Context.User as IGuildUser).VoiceChannel;
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (clientVoiceChannel == null) { await userChannel.SendMessageAsync("I'm not connected to any voice channel"); return; }

            await _audioService.SkipTrackAsync(Context.Channel, guild, songsToSkip);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops the music and clears the queue.")]
        [Usage("stop")]
        public async Task StopVoiceActivity()
        {
            var userChannel = (Context.User as IGuildUser).VoiceChannel;
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser?.VoiceChannel;

            if (clientVoiceChannel == null) { await userChannel.SendMessageAsync("I'm not connected to any voice channel"); return; }

            await _audioService.StopVoiceActivity(Context.Channel, guild);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pauses the audio streaming")]
        [Usage("pause")]
        public async Task Pause()
        {
            var userChannel = (Context.User as IGuildUser).VoiceChannel;
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (clientVoiceChannel == null) { return; }

            await _audioService.PauseOrResume(Context.Channel, guild, pause: true);

        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes the audio streaming")]
        [Usage("resume")]
        public async Task Resume()
        {
            var userChannel = (Context.User as IGuildUser).VoiceChannel;
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser.VoiceChannel;

            if (clientVoiceChannel == null) { return; }

            await _audioService.PauseOrResume(Context.Channel, guild, pause: false);

        }

        [Command("queue", RunMode = RunMode.Async)]
        [Summary("Shows all the queued songs")]
        [Usage("queue")]
        [Alias("q")]
        public async Task ShowQueue()
        {
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser?.VoiceChannel;

            if (clientVoiceChannel == null) { await ReplyAsync("I'm not connected to any voice channel"); return; }

            await ReplyAsync(embed: await _audioService.QueueEmbed(guild));
        }

        [Command("move", RunMode = RunMode.Async)]
        [Summary("Moves a song from the current to the desired position")]
        [Usage("move currentPos: 1 newPos: 3")]
        public async Task MoveTrack(MoveTrackModel moveTrackModel)
        {
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser?.VoiceChannel;

            if (clientVoiceChannel == null) { await ReplyAsync("I'm not connected to any voice channel"); return; }

            if (moveTrackModel.CurrentPos == 0 && moveTrackModel.NewPos == 0)
            {
                await ReplyAsync("No changes made");
                return;
            }

            await _audioService.MoveTrack(Context.Channel, guild, moveTrackModel);
        }

        [Command("removebetween", RunMode = RunMode.Async)]
        [Summary("Removes tracks within the specified range. By default, the provided indexes are not included. Type `include: true` to include them")]
        [Usage("removebetween startPos: 10 endPos:15 include: true")]
        [Alias("rmbtw")]
        public async Task RemoveBetween(RemoveBetweenModel removeBetweenModel)
        {
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser?.VoiceChannel;

            if (clientVoiceChannel == null) { await ReplyAsync("I'm not connected to any voice channel"); return; }

            if (removeBetweenModel.StartPos == 0 && removeBetweenModel.EndPos == 0)
            {
                await ReplyAsync("No changes made");
                return;
            }

            await _audioService.RemoveBetween(Context.Channel, guild, removeBetweenModel);
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a track at the specified index")]
        [Usage("remove 10")]
        [Alias("rm")]
        public async Task RemoveTrack(int? trackPos)
        {
            var guild = Context.Guild;
            var clientVoiceChannel = guild.CurrentUser?.VoiceChannel;

            if (clientVoiceChannel == null) { await ReplyAsync("I'm not connected to any voice channel"); return; }

            await _audioService.RemoveTrack(Context.Channel, guild, trackPos);
        }
    }
}
