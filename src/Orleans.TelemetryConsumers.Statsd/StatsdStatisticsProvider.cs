using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Orleans.Telemetry
{
    public class StatsdStatisticsProvider : StatsdProvider, IConfigurableSiloMetricsDataPublisher, IStatisticsPublisher, IProvider
    {
        public StatsdStatisticsProvider()
        {
            StatsdConfiguration.CheckConfiguration();
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            _state.DeploymentId = deploymentId;
            _state.SiloName = siloName;
            _state.GatewayAddress = gateway.ToString();
            _state.HostName = hostName;

            return TaskDone.Done;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            _state.DeploymentId = deploymentId;
            _state.IsSilo = isSilo;
            _state.SiloName = siloName;
            _state.Address = address;
            _state.HostName = hostName;

            return TaskDone.Done;
        }

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
            {
                _logger.Verbose3($"{ nameof(StatsdStatisticsProvider)}.ReportMetrics called with metrics: {0}, name: {1}, id: {2}.", metricsData, _state.SiloName, _state.Id);
            }

            try
            {
                SendSiloMetrics(metricsData);
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                {
                    _logger.Verbose($"{ nameof(StatsdStatisticsProvider)}.ReportMetrics failed: {0}", ex);
                }

                throw;
            }

            return TaskDone.Done;
        }

        private static void SendSiloMetrics(ISiloPerformanceMetrics metricsData)
        {
            Metrics.GaugeAbsoluteValue("activations_count", metricsData.ActivationCount);
            Metrics.GaugeAbsoluteValue("recently_used_activations", metricsData.RecentlyUsedActivationCount);
            Metrics.GaugeAbsoluteValue("request_queue_length", metricsData.RequestQueueLength);
            Metrics.GaugeAbsoluteValue("is_overloaded", metricsData.IsOverloaded ? 1 : 0);
            Metrics.GaugeAbsoluteValue("client_count", metricsData.ClientCount);

            SendCoreMetrics(metricsData);
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
        public Task ReportStats(List<ICounter> statsCounters)
        {
            if (_logger != null && _logger.IsVerbose3)
            {
                _logger.Verbose3($"{ nameof(StatsdStatisticsProvider)}.ReportStats called with {0} counters, name: {1}, id: {2}", statsCounters.Count, _state.SiloName, _state.Id);
            }

            try
            {
                var counters = statsCounters.Where(cs => cs.Storage == CounterStorage.LogAndTable);

                foreach (var counter in counters)
                {
                    SendStats(counter);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                {
                    _logger.Verbose($"{ nameof(StatsdStatisticsProvider)}.ReportStats failed: {0}", ex);
                }

                throw;
            }

            return TaskDone.Done;
        }

        private static void SendStats(ICounter counter)
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
