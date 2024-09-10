using Discord;
using DiscordMusicBot.Common;
using DiscordMusicBot.Source;
using DiscordMusicBotNetCore.CommandsModels;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;

namespace DiscordMusicBot.Utils
{
    public static class Utils
    {
        public static async Task<string> ConvertDuration(double duration)
        {
            TimeSpan durationString = TimeSpan.FromSeconds(duration);
            return durationString.ToString(@"hh\:mm\:ss");
        }

        public static async Task<Embed> CurrentTrackEmbed(string trackTitle, string trackDuration, string thumbnailUrl)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder.AddField(trackTitle, trackDuration)
                .WithThumbnailUrl(thumbnailUrl)
                .WithTitle("Now Playing :notes:")
                .WithColor(Color.Orange);

            return embedBuilder.Build();
        }

        public static async Task<Embed> QueuedTrackEmbed(string trackTitle, string thumbnailUrl, int queueCount)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.Title = "Queued Song";
            builder.Description = $"{trackTitle}\n\nPosition #{queueCount.ToString()}";
            builder.ThumbnailUrl = thumbnailUrl;

            return builder.Build();
        }

        public static async Task<bool> IsUrl(string query)
        {
            return Uri.IsWellFormedUriString(query, UriKind.Absolute);
        }

        public static async Task<List<AudioTrack>> ExtractTracksMetadata(string query, bool isUrl = true)
        {
            List<AudioTrack> tracksInfo = new List<AudioTrack>();

            if (!isUrl)
            {
                query = HttpUtility.UrlEncode(query);
            }

            string arguments = $"/c yt-dlp --dump-single-json --skip-download --flat-playlist ytsearch:{query}";

            if (isUrl)
            {
                arguments = $"/c yt-dlp --dump-single-json --skip-download --yes-playlist --flat-playlist \"{query}\"";
            }

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = $"cmd.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo = processInfo;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();

                    JObject json = JObject.Parse(output);
                    AudioTrack trackToSave;

                    if (json.ContainsKey("entries"))
                    {
                        if (isUrl)
                        {
                            foreach (JObject song in json["entries"].Value<JArray>())
                            {
                                trackToSave = new AudioTrack();
                                AudioTrackInfo trackInfo = AudioTrackInfo.ExtractTrackMetadata(song);

                                if (trackInfo == null)
                                {
                                    continue;
                                }

                                trackToSave.AudioTrackInfo = trackInfo;
                                tracksInfo.Add(trackToSave);
                            }
                        }
                        else
                        {
                            JObject firstVideoJson = json["entries"].Value<JArray>()[0].Value<JObject>();
                            trackToSave = new AudioTrack();
                            AudioTrackInfo trackInfo = AudioTrackInfo.ExtractTrackMetadata(firstVideoJson);

                            if (trackInfo != null)
                            {
                                trackToSave.AudioTrackInfo = trackInfo;
                                tracksInfo.Add(trackToSave);
                            }

                        }
                    }
                    else
                    {
                        trackToSave = new AudioTrack();

                        AudioTrackInfo trackInfo = AudioTrackInfo.ExtractTrackMetadata(json);

                        if (trackInfo != null)
                        {
                            trackToSave.AudioTrackInfo = AudioTrackInfo.ExtractTrackMetadata(json);
                            tracksInfo.Add(trackToSave);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
            return tracksInfo;
        }

        public static async Task<Embed> FullQueueEmbed(List<AudioTrack> tracks, string title)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = $"{title} :notes:";
            
            string description = string.Empty;
            int i = 1;

            if (!tracks.Any())
            {
                description = ":x: Queue is empty";
            }
            else
            {
                foreach (AudioTrack track in tracks)
                {
                    description += $"{i}. **{track.AudioTrackInfo.Title}**\n";
                    i++;
                    if (i > 20)
                        break;
                }
            }

            string footerText = tracks.Count > i ? $"There are {tracks.Count - i} more tracks in queue" : "No more tracks in queue";
            builder.Description = description;
            builder.WithFooter(new EmbedFooterBuilder
            {
                Text = footerText
            });
            
            return builder.Build();
        }

        public static async Task<(Queue<AudioTrack>, string)> MoveTrack(Queue<AudioTrack> queue, int currentPos, int newPos)
        {
            
            List<AudioTrack> tracks = queue.ToList();
            AudioTrack track = tracks.ElementAt(currentPos - 1);
            
            tracks.Remove(track);
            tracks.Insert(newPos - 1, track);
            
            return (new Queue<AudioTrack>(tracks), $"{track.AudioTrackInfo.Title} is now at position #{newPos}");
        }

        public static async Task<(Queue<AudioTrack>, string)> RemoveBetween(Queue<AudioTrack> queue, RemoveBetweenModel removeBetweenModel)
        {

            List<AudioTrack> tracks = queue.ToList();

            if (removeBetweenModel.Include)
            {
                tracks.RemoveRange(removeBetweenModel.StartPos - 1, ((removeBetweenModel.EndPos - removeBetweenModel.StartPos)+1));
            }
            else
            {
                tracks.RemoveRange(removeBetweenModel.StartPos, ((removeBetweenModel.EndPos - removeBetweenModel.StartPos))-1);
            }


            bool isGreater = removeBetweenModel.EndPos > removeBetweenModel.StartPos;

            int removedTracks = removeBetweenModel.Include ? (removeBetweenModel.EndPos - removeBetweenModel.StartPos + 1) : (removeBetweenModel.EndPos - removeBetweenModel.StartPos - 1);

            return (new Queue<AudioTrack>(tracks), $"Removed {removedTracks} " + (isGreater ? "tracks" : "track"));
        }
        
        public static async Task<Queue<AudioTrack>> RemoveTrack(Queue<AudioTrack> queue, int trackPos)
        {
            List<AudioTrack> trackList = queue.ToList();
            trackList.RemoveAt(trackPos - 1);

            Queue<AudioTrack> newQueue = new Queue<AudioTrack>(trackList);

            return newQueue;
        }
    }
}
