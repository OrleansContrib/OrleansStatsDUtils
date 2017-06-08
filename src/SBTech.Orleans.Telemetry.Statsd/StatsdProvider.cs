using System;
using System.Threading.Tasks;
using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;

namespace SBTech.Orleans.Telemetry.Statsd
{
    class State
    {
        public string DeploymentId { get; set; } = "";
        public bool IsSilo { get; set; } = true;
        public string SiloName { get; set; } = "";
        public string Id { get; set; } = "";
        public string Address { get; set; } = "";
        public string GatewayAddress { get; set; } = "";
        public string HostName { get; set; } = "";
        public string StatsDServerName { get; set; } = "127.0.0.1";
        public int StatsDServerPort { get; set; } = 8125;
        public string StatsDPrefix { get; set; } = "";
        public int StatsDMaxUdpPacketSize { get; set; } = 512;
        public Guid ServiceId { get; set; } = Guid.Empty;
    }

    public abstract class StatsdProvider : IProvider
    {
        internal readonly State State = new State();

        /// <summary>
        /// Initialization of StatsdStatisticsProvider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="providerRuntime"></param>
        /// <param name="providerConfiguration"></param>
        /// <returns></returns>
        public virtual Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration providerConfiguration)
        {
            Name = name;

            if (providerRuntime.ServiceProvider != null)
            {
                State.Id = providerRuntime.SiloIdentity;
                State.ServiceId = providerRuntime.ServiceId;
            }
            
            if (providerConfiguration.Properties.ContainsKey("StatsDServerName"))
                State.StatsDServerName = providerConfiguration.Properties["StatsDServerName"];

            if (providerConfiguration.Properties.ContainsKey("StatsDServerPort"))
                State.StatsDServerPort = int.Parse(providerConfiguration.Properties["StatsDServerPort"]);

            if (providerConfiguration.Properties.ContainsKey("StatsDPrefix"))
                State.StatsDPrefix = providerConfiguration.Properties["StatsDPrefix"];

            if (providerConfiguration.Properties.ContainsKey("StatsDMaxUdpPacketSize"))
                State.StatsDMaxUdpPacketSize = int.Parse(providerConfiguration.Properties["StatsDMaxUdpPacketSize"]);

            var config = new MetricsConfig
            {
                StatsdServerName = State.StatsDServerName,
                StatsdServerPort = State.StatsDServerPort,
                Prefix = string.IsNullOrEmpty(State.StatsDPrefix)
                    ? $"{State.HostName.ToLower()}.{State.SiloName.ToLower()}"
                    : $"{State.StatsDPrefix.ToLower()}.{State.HostName.ToLower()}",
                StatsdMaxUDPPacketSize = State.StatsDMaxUdpPacketSize
            };

            Metrics.Configure(config);

            return Task.CompletedTask;
        }

        protected static void SendCoreMetrics(ICorePerformanceMetrics metricsData)
        {
            //Todo: replace to constants
            Metrics.GaugeAbsoluteValue("cpu_usage", metricsData.CpuUsage);
            Metrics.GaugeAbsoluteValue("total_physical_memory", metricsData.TotalPhysicalMemory);
            Metrics.GaugeAbsoluteValue("available_physical_memory", metricsData.AvailablePhysicalMemory);
            Metrics.GaugeAbsoluteValue("memory_usage", metricsData.MemoryUsage);
            Metrics.GaugeAbsoluteValue("total_physical_memory", metricsData.TotalPhysicalMemory);
            Metrics.GaugeAbsoluteValue("send_queue_length", metricsData.SendQueueLength);
            Metrics.GaugeAbsoluteValue("receive_queue_length", metricsData.ReceiveQueueLength);
            Metrics.GaugeAbsoluteValue("sent_messages", metricsData.SentMessages);
            Metrics.GaugeAbsoluteValue("received_messages", metricsData.ReceivedMessages);
        }

        /// <summary>
        /// Name of the provider
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Closes provider
        /// </summary>        
        public Task Close() => Task.CompletedTask;
    }
}