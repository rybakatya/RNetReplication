

using Newtonsoft.Json;
using RapidNet;
using RapidNet.Connections;
using RapidNet.Logging;

namespace RNetZoneServer
{
    public class ZoneServer : StandaloneServerLibrary.Zones.RNetZoneServer
    {
        protected override void OnSocketConnect(Connection connection)
        {
           
        }

        protected override void OnSocketDisconnect(Connection connection)
        {
            
        }

        protected override void OnSocketReceive(Connection connection, ushort messageID, nint messageData)
        {
            
        }

        protected override void OnSocketTimeout(Connection connection)
        {
            
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
