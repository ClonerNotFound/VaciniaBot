using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VaciniaBot.config
{
    public class JSONReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public List<ulong> AdminRoles { get; set; }
        public ulong LogChannelId { get; set; }
        public ulong ConsoleChannelId { get; set; }
        public MySQLConfig MySQL { get; set; }

        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                Token = data.Token;
                Prefix = data.Prefix;
                AdminRoles = data.AdminRoles;
                LogChannelId = data.LogChannelId;
                ConsoleChannelId = data.ConsoleChannelId;
                MySQL = data.MySQL;
            }
        }

        private class JSONStructure
        {
            public string Token { get; set; }
            public string Prefix { get; set; }
            public List<ulong> AdminRoles { get; set; }
            public ulong LogChannelId { get; set; }
            public ulong ConsoleChannelId { get; set; }
            public MySQLConfig MySQL { get; set; }
        }

        public class MySQLConfig
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Database { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public string Table { get; set; }
            public string Column { get; set; }
        }
    }
}