using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordBot.config
{
    public class JSONreader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong rolemention { get; set; }
        public ulong channelsend { get; set; }
        public ulong roleidforbtn { get; set; }
        public ulong rolementionnr { get; set; }
        public ulong channelsendnr { get; set; }
        public ulong roleidforbtnnr { get; set; }
        public ulong channelForNr { get; set; }
        public ulong ChannelForMrp { get; set; }

        public async Task ReadJson()
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Config file not found at {configPath}");
            }

            string json = await File.ReadAllTextAsync(configPath);
            var config = JsonConvert.DeserializeObject<JSONstructure>(json) ??
                throw new InvalidOperationException("Invalid config file format");

            token = config.token ?? throw new ArgumentNullException(nameof(config.token));
            prefix = config.prefix ?? "!";
            rolemention = config.rolemention;
            channelsend = config.channelsend;
            roleidforbtn = config.roleidforbtn;

            rolementionnr = config.roleidforbtnnr;
            channelsendnr = config.channelsendnr;
            roleidforbtnnr = config.roleidforbtnnr;

            channelForNr = config.channelForNr;
            ChannelForMrp = config.ChannelForMrp;
        }
    }

    internal sealed class JSONstructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong rolemention { get; set; }
        public ulong channelsend { get; set; }
        public ulong roleidforbtn { get; set; }
        public ulong rolementionnr { get; set; }
        public ulong channelsendnr { get; set; }
        public ulong roleidforbtnnr { get; set; }
        public ulong channelForNr { get; set; }
        public ulong ChannelForMrp { get; set; }
    }
}