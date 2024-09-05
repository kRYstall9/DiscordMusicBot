using Newtonsoft.Json.Linq;

namespace DiscordMusicBot.Common
{
    public class AudioTrackInfo
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public double Duration { get; set; }
        public string ThumbnailUrl { get; set; }

        //ATM there's no audio track's author in the json returned by yt-dlp
        //public string Author { get; set; }

        public static AudioTrackInfo ExtractTrackMetadata(JObject json)
        {
            if (String.IsNullOrEmpty(json["duration"].Value<string>())) return null;

            return new()
            {
                Title = json["title"].Value<string>(),
                Duration = json["duration"].Value<double>(),
                ThumbnailUrl = json["thumbnails"][0]["url"].Value<string>(),
                Url = json.ContainsKey("url") ? json["url"].Value<string>() : json["webpage_url"].Value<string>()
            };
        }
    }
}
