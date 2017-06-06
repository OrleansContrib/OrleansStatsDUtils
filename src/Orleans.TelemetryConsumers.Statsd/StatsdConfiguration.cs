using StatsdClient;
using System;

namespace Orleans.Telemetry
{
    public static class StatsdConfiguration
    {
        static bool _configured = false;

        /// <summary>
        /// Initialize statsd settings for writing metrics
        /// </summary>
        /// <param name="statsdHost">statsd server address</param>
        /// <param name="statsdPort">statsd server port</param>
        /// <param name="siloName">Silo name</param>
        /// <param name="prefix">Prefix for metrics</param>
        /// <param name="hostName">Address of orleans host</param>
        /// <param name="maxUdpPacketSize">Maximum size of UDP packets</param>
        /// <param name="useTcpProtocol">Use TCP protocol</param>
        public static void Initialize(string statsdHost, int statsdPort, string siloName, string prefix, 
                                      string hostName, int maxUdpPacketSize = 512, bool useTcpProtocol = false)
        {
            Metrics.Configure(new MetricsConfig
            {
                StatsdServerName = statsdHost,
                StatsdServerPort = statsdPort,
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
