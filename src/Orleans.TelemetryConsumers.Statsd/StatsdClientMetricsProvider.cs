using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Orleans.Telemetry
{
    public class StatsdClientMetricsProvider : StatsdProvider, IConfigurableClientMetricsDataPublisher, IStatisticsPublisher
    {
        private int MAX_BULK_UPDATE_DOCS = 200;

        public StatsdClientMetricsProvider()
        {
            StatsdConfiguration.CheckConfiguration();
        }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            _state.DeploymentId = deploymentId;
            _state.Id = clientId;
            _state.Address = address.ToString();
            _state.HostName = hostName;
        }
        
        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            _state.Id = clientId;
            _state.Address = address.ToString();

            return TaskDone.Done;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName) => TaskDone.Done;

        /// <summary>
        /// Metrics for client
        /// </summary>        
        public async Task ReportMetrics(IClientPerformanceMetrics metricsData)
        {
            if (_logger != null && _logger.IsVerbose3)
                _logger.Verbose3($"{nameof(StatsdClientMetricsProvider)}.ReportMetrics called with metrics: {0}, name: {1}, id: {2}.", metricsData, _state.SiloName, _state.Id);

            try
            {
                SendClientPerformanceMetrics(metricsData);
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                    _logger.Verbose($"{ nameof(StatsdClientMetricsProvider)}.ReportMetrics failed: {0}", ex);

                throw;
            }
        }

        private void SendClientPerformanceMetrics(IClientPerformanceMetrics metricsData)
        {
            SendCoreMetrics(metricsData);

            Metrics.GaugeAbsoluteValue("connected_gateway_count", metricsData.ConnectedGatewayCount);
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
        public async Task ReportStats(List<ICounter> statsCounters)
        {
            if (_logger != null && _logger.IsVerbose3)
            {
                _logger.Verbose3($"{ nameof(StatsdClientMetricsProvider)}.ReportStats called with {0} counters, name: {1}, id: {2}", statsCounters.Count, _state.SiloName, _state.Id);
            }

            try
            {
                foreach (var counter in statsCounters)
                {
                    SendStats(counter);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null && _logger.IsVerbose)
                {
                    _logger.Verbose($"{ nameof(StatsdClientMetricsProvider)}.ReportStats failed: {0}", ex);
                }

                throw;
            }
        }

        private void SendStats(ICounter counter)
        {
            var valueStr = counter.IsValueDelta
                ? counter.GetDeltaString()
                : counter.GetValueString();

            float value;

            if (float.TryParse(valueStr, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                var counterName = counter.Name.ToLowerInvariant();

                if (counter.IsValueDelta)
                {
                    Metrics.GaugeDelta(counterName, value);
                }
                else
                {
                    Metrics.GaugeAbsoluteValue(counterName, value);
                }
            }
        }
    }
}
