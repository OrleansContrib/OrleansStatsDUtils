using StatsdClient;
using System;

namespace Orleans.Telemetry
{
    public static class StatsdConfiguration
    {
        static bool _configured = false;

        public static void Initialize(string serverName, int port, string siloName, string prefix, 
                                      string hostName, int maxUdpPacketSize = 512, bool useTcpProtocol = false)
        {
            Metrics.Configure(new MetricsConfig
            {
                StatsdServerName = serverName,
                StatsdServerPort = port,
                Prefix = string.IsNullOrEmpty(prefix)
                    ? $"{hostName.ToLower()}.{siloName.ToLower()}"
                    : $"{prefix.ToLower()}.{hostName.ToLower()}",
                StatsdMaxUDPPacketSize = maxUdpPacketSize,
                UseTcpProtocol = useTcpProtocol
            });

            _configured = true;
        }

        public static void CheckConfiguration()
        {
            if (!_configured)
            {
                throw new Exception("You should call StatsdConfiguration.Initialize(...) first");
            }
        }
    }
}
