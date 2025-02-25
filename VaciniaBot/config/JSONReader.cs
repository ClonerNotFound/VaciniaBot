using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaciniaBot.config
{
    public class JSONReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public List<ulong> AdminRoles { get; set; }
        public ulong LogChannelId { get; set; }
        public ulong СonsoleChannel { get; set; }
        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONStructure>(json);

                Token = data.Token;
                Prefix = data.Prefix;
                AdminRoles = data.AdminRoles;
                LogChannelId = data.LogChannelId;
                СonsoleChannel = data.СonsoleChannel;
            }
        }
        private class JSONStructure
        {
            public string Token { get; set; }
            public string Prefix { get; set; }
            public List<ulong> AdminRoles { get; set; }
            public ulong LogChannelId { get; set; }
            public ulong СonsoleChannel { get; set; }
        }
    }
}
