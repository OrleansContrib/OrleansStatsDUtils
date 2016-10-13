using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;

namespace SBTech.OrleansStatsDUtils
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
    }

    public class StatsDStatisticsProvider : IConfigurableSiloMetricsDataPublisher,
                                            IStatisticsPublisher,
                                            IProvider
    {   
        State _state = new State();
        Logger _logger;

        /// <summary>
        /// Name of the provider
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Closes provider
        /// </summary>        
        public Task Close() => TaskDone.Done;

        /// <summary>
        /// Initialization of StatsDStatisticsProvider
        /// </summary>        
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            _state.Id = providerRuntime.SiloIdentity;
            _logger = providerRuntime.GetLogger(typeof(StatsDStatisticsProvider).Name);

            if (config.Properties.ContainsKey("StatsDServerName"))
                _state.StatsDServerName = config.Properties["StatsDServerName"];

            if (config.Properties.ContainsKey("StatsDServerPort"))
                _state.StatsDServerPort = int.Parse(config.Properties["StatsDServerPort"]);
            
            if (config.Properties.ContainsKey("StatsDPrefix"))
                _state.StatsDPrefix = config.Properties["StatsDPrefix"];

            if (config.Properties.ContainsKey("StatsDMaxUdpPacketSize"))
                _state.StatsDMaxUdpPacketSize = int.Parse(config.Properties["StatsDMaxUdpPacketSize"]);

            var metricsConfig = new MetricsConfig
            {
                StatsdServerName = _state.StatsDServerName,
                StatsdServerPort = _state.StatsDServerPort,
                Prefix = string.IsNullOrEmpty(_state.StatsDPrefix)
                            ? $"{_state.HostName.ToLower()}.{_state.SiloName.ToLower()}"
                            : $"{_state.StatsDPrefix.ToLower()}.{_state.HostName.ToLower()}",
                StatsdMaxUDPPacketSize = _state.StatsDMaxUdpPacketSize
            };

            Metrics.Configure(metricsConfig);

            return TaskDone.Done;
        }

        /// <summary>
        /// Initialization of configuration for Silo
        /// </summary>        
        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            _state.DeploymentId = deploymentId;
            _state.IsSilo = isSilo;
            _state.SiloName = siloName;
            _state.Address = address.ToString();
            _state.GatewayAddress = gateway.ToString();
            _state.HostName = hostName;
        }

        /// <summary>
        /// Metrics for Silo
        /// </summary>        
        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            if (_logger != null && _logger.IsVerbose3)
                _logger.Verbose3("StatsDStatisticsProvider.ReportMetrics called with metrics: {0}, name: {1}, id: {2}.", metricsData, _state.SiloName, _state.Id);

            try
            {
                SendSiloMetrics(metricsData);
                return TaskDone.Done;
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                    _logger.Verbose("StatsDStatisticsProvider.ReportMetrics failed: {0}", ex);

                throw;
            }
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
        public Task ReportStats(List<ICounter> statsCounters)
        {
            if (_logger != null && _logger.IsVerbose3)
                _logger.Verbose3("StatsDStatisticsProvider.ReportStats called with {0} counters, name: {1}, id: {2}", statsCounters.Count, _state.SiloName, _state.Id);

            try
            {
                foreach (var counter in statsCounters.Where(cs => cs.Storage == CounterStorage.LogAndTable))
                {
                    SendStats(counter);
                }
                return TaskDone.Done;
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                    _logger.Verbose("StatsDStatisticsProvider.ReportStats failed: {0}", ex);

                throw;
            }
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName) => TaskDone.Done;

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName) => TaskDone.Done;

        static void SendSiloMetrics(ISiloPerformanceMetrics metricsData)
        {
            Metrics.GaugeAbsoluteValue("cpu_usage", metricsData.CpuUsage);
            Metrics.GaugeAbsoluteValue("total_physical_memory", metricsData.TotalPhysicalMemory);
            Metrics.GaugeAbsoluteValue("available_physical_memory", metricsData.AvailablePhysicalMemory);
            Metrics.GaugeAbsoluteValue("memory_usage", metricsData.MemoryUsage);
            Metrics.GaugeAbsoluteValue("memory_usage", metricsData.MemoryUsage);
            Metrics.GaugeAbsoluteValue("total_physical_memory", metricsData.TotalPhysicalMemory);
            Metrics.GaugeAbsoluteValue("send_queue_length", metricsData.SendQueueLength);
            Metrics.GaugeAbsoluteValue("receive_queue_length", metricsData.ReceiveQueueLength);
            Metrics.GaugeAbsoluteValue("sent_messages", metricsData.SentMessages);
            Metrics.GaugeAbsoluteValue("received_messages", metricsData.ReceivedMessages);
            Metrics.GaugeAbsoluteValue("activations_count", metricsData.ActivationCount);
            Metrics.GaugeAbsoluteValue("recently_used_activations", metricsData.RecentlyUsedActivationCount);
            Metrics.GaugeAbsoluteValue("request_queue_length", metricsData.RequestQueueLength);
            Metrics.GaugeAbsoluteValue("is_overloaded", metricsData.IsOverloaded ? 1 : 0);
            Metrics.GaugeAbsoluteValue("client_count", metricsData.ClientCount);
        }

        static void SendStats(ICounter counter)
        {
            var valueStr = counter.IsValueDelta 
                ? counter.GetDeltaString() 
                : counter.GetValueString();

            float value;
            if (float.TryParse(valueStr, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                var counterName = counter.Name.ToLowerInvariant();

                if (counter.IsValueDelta)
                    Metrics.GaugeDelta(counterName, value);
                else
                    Metrics.GaugeAbsoluteValue(counterName, value);
            }
        }
    }
}
