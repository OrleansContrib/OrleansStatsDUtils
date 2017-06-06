using System;
using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;
using System.Threading.Tasks;

namespace Orleans.Telemetry
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

        public StatsdProvider()
        {
            StatsdConfiguration.CheckConfiguration();
        }

        /// <summary>
        /// Initialization of StatsdStatisticsProvider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="providerRuntime"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            State.Id = providerRuntime.SiloIdentity;
            State.ServiceId = providerRuntime.ServiceId;
            
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