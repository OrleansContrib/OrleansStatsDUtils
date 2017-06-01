using StatsdClient;
using System;

namespace Orleans.Telemetry
{
    public static class StatsdConfiguration
    {
        private static bool _configured = false;

        public static void Initialize(string serverName, int port, string prefix, int maxUdpPacketSize, string hostName, string siloName)
        {
            Metrics.Configure(new MetricsConfig
            {
                StatsdServerName = serverName,
                StatsdServerPort = port,
                Prefix = string.IsNullOrEmpty(prefix)
                    ? $"{hostName.ToLower()}.{siloName.ToLower()}"
                    : $"{prefix.ToLower()}.{hostName.ToLower()}",
                StatsdMaxUDPPacketSize = maxUdpPacketSize
            });
        }

        public static bool IsConfigured()
        {
            return _configured;
        }

        public static void CheckConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
