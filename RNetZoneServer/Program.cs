

using Newtonsoft.Json;
using RapidNet;
using RapidNet.Connections;
using RapidNet.Logging;

namespace RNetZoneServer
{
    public struct ZoneServerSettings
    {
        public string ip;
        public ushort port;
        public byte maxChannels;
        public ushort maxConnections;
        public List<RNetIPAddress> zoneProxyServers;

    }

    internal class ZoneServer
    {
        private bool isInit = false;
        public bool IsInit => isInit;

        private readonly ZoneServerSettings settings;


        private List<Connection> proxyServers;
        public ZoneServer()
        {
            if (File.Exists("settings.json") == false)
            {
                var set = new ZoneServerSettings()
                {
                    ip = "127.0.0.1",
                    port = 7777,
                    maxChannels = 255,
                    maxConnections = 1024,
                    zoneProxyServers = new List<RNetIPAddress>() {
                        new RNetIPAddress("127.0.0.1", 7778),
                        new RNetIPAddress("127.0.0.1", 7779)
                    }
                };
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(set, Formatting.Indented));
            }

            var json = File.ReadAllText("settings.json");

            settings = JsonConvert.DeserializeObject<ZoneServerSettings>(json);
            proxyServers = new List<Connection>((int)settings.maxConnections);
            RNet.Init(onInit);
        }

        internal void Tick()
        {
            RNet.Tick();
        }


        private void onInit()
        {
            isInit = true;
            RNet.RegisterOnSocketConnectEvent(LogicSocketConnect);
            RNet.InitializeServer(settings.ip, settings.port,settings.maxChannels, settings.maxConnections);
        }

        private bool LogicSocketConnect(Connection connection)
        {
            var ip = new RNetIPAddress(connection.IpAddress.ToString(), connection.Port);
            if(settings.zoneProxyServers.Contains(ip) == true)
            {
                proxyServers.Add(connection);
                Logger.Log(LogLevel.Info, "A Zone ProxyServer has connected!");
                return true;
            }

            return true;
        }
    }
    internal class Program
    {
        
        static void Main(string[] args)
        {
            ZoneServer server = new ZoneServer();
            while(true)
            {
                    server.Tick();
            }
        }

       
    }
}
