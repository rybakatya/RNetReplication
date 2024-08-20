using RapidNet;
using System.Collections.Generic;


namespace StandaloneServerLibrary.Zones
{
    public struct ZoneServerSettings
    {
        public string ip;
        public ushort port;
        public byte maxChannels;
        public ushort maxConnections;
        public List<RNetIPAddress> zoneProxyServers;

    }
}
