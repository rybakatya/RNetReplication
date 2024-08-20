using RapidNet;
using RapidNet.Connections;

namespace RNetZoneProxy
{
    internal class RNetZoneProxyServer
    {
        private bool isInit = false;
        public RNetZoneProxyServer()
        {
            RNet.Init(onInit);
        }

        public void Tick()
        {
            RNet.Tick();
        }

        private void onInit()
        {
            isInit = true;
            RNet.RegisterOnSocketConnectEvent(LogicSocketConnect);
            RNet.InitializeServer("127.0.0.1", 7778, 255, 1024);
            RNet.Connect("127.0.0.1", 7777);
        }

        private bool LogicSocketConnect(Connection connection)
        {
            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var server = new RNetZoneProxyServer();
            while(true)
            {
                server.Tick();
            }
        }
    }
}
