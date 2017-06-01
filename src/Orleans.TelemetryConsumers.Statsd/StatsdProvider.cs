using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;
using System;
using System.Threading.Tasks;

namespace Orleans.Telemetry
{
    public abstract class StatsdProvider : IProvider
    {
        public const string StatsdHostAddress = "StatsdHostAddress";
        public const string StatsdPort = "StatsdPort";
        public const string StatsdPrefix = "StatsdPrefix";
        public const string StatsdCultureInfo = "StatsdCultureInfo";
        
        protected Logger _logger;

        internal readonly State _state = new State();

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

            _state.Id = providerRuntime.SiloIdentity;
            _state.ServiceId = providerRuntime.ServiceId;
            _logger = providerRuntime.GetLogger(typeof(StatsdStatisticsProvider).Name);
          
            return TaskDone.Done;
        }

        protected static void SendCoreMetrics(ICorePerformanceMetrics metricsData)
        {
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
        public Task Close() => TaskDone.Done;
    }
}
