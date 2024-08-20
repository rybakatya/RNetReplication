using Newtonsoft.Json;
using RapidNet;
using RapidNet.Connections;
using RapidNet.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace StandaloneServerLibrary.Zones
{

    public abstract class RNetZoneServer
    {
        private bool isInit = false;
        public bool IsInit => isInit;

        private readonly ZoneServerSettings settings;


        private List<Connection> proxyServers;
        public RNetZoneServer()
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
            RNet.RegisterOnSocketDisconnectEvent(LogicSocketDisconnect);
            RNet.RegisterOnSocketTimeoutEvent(LogicSocketTimeout);
            RNet.RegisterReceiveEvent(LogicReceiveEvent);
            RNet.InitializeServer(settings.ip, settings.port, settings.maxChannels, settings.maxConnections);
        }

        private bool LogicReceiveEvent(Connection connection, ushort messageID, IntPtr messageData)
        {
            OnSocketReceive(connection, messageID, messageData);
            return true;
        }

        private bool LogicSocketTimeout(Connection connection)
        {
            OnSocketTimeout(connection);
            return true;
        }

        private bool LogicSocketDisconnect(Connection connection)
        {
            OnSocketDisconnect(connection);
            return true;
        }

        private bool LogicSocketConnect(Connection connection)
        {
            var ip = new RNetIPAddress(connection.IpAddress.ToString(), connection.Port);
            if (settings.zoneProxyServers.Contains(ip) == true)
            {
                proxyServers.Add(connection);
                Logger.Log(LogLevel.Info, "A Zone ProxyServer has connected!");
            }
            OnSocketConnect(connection);            
            return true;
        }

        protected abstract void OnSocketConnect(Connection connection);
        protected abstract void OnSocketDisconnect(Connection connection);
        protected abstract void OnSocketTimeout(Connection connection);
        protected abstract void OnSocketReceive(Connection connection, ushort messageID, IntPtr messageData);

    }
}
