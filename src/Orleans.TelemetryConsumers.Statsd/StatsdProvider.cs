using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;
using System.Threading.Tasks;

namespace Orleans.Telemetry
{
    public abstract class StatsdProvider : IProvider
    {
        protected Logger Logger;

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
            Logger = providerRuntime.GetLogger(typeof(StatsdStatisticsProvider).Name);

            return TaskDone.Done;
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
        public Task Close() => TaskDone.Done;
    }
}
