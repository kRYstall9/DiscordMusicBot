namespace DiscordMusicBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UsageAttribute : Attribute
    {
        public string Usage { get; }


        public UsageAttribute(string usage)
        {
            this.Usage = $"`{usage}`";
        }
    }
}
