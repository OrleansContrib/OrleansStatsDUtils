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
    public class StatsdClientMetricsProvider :
        StatsdProvider,
        IConfigurableClientMetricsDataPublisher,
        IStatisticsPublisher
    {
        public StatsdClientMetricsProvider()
        {
            StatsdConfiguration.CheckConfiguration();
        }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            State.DeploymentId = deploymentId;
            State.Id = clientId;
            State.Address = address.ToString();
            State.HostName = hostName;
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            State.Id = clientId;
            State.Address = address.ToString();

            return TaskDone.Done;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName) => TaskDone.Done;

        /// <summary>
        /// Metrics for client
        /// </summary>        
        public Task ReportMetrics(IClientPerformanceMetrics metricsData)
        {
            if (Logger != null && Logger.IsVerbose3)
                Logger.Verbose3($"{nameof(StatsdClientMetricsProvider)}.ReportMetrics called with metrics: {0}, name: {1}, id: {2}.", metricsData, State.SiloName, State.Id);

            try
            {
                SendClientPerformanceMetrics(metricsData);
            }
            catch (Exception ex)
            {
                if (Logger != null && Logger.IsVerbose)
                    Logger.Verbose($"{ nameof(StatsdClientMetricsProvider)}.ReportMetrics failed: {0}", ex);

                throw;
            }

            return TaskDone.Done;
        }

        private static void SendClientPerformanceMetrics(IClientPerformanceMetrics metricsData)
        {
            SendCoreMetrics(metricsData);

            Metrics.GaugeAbsoluteValue("connected_gateway_count", metricsData.ConnectedGatewayCount);
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
        public Task ReportStats(List<ICounter> statsCounters)
        {
            if (Logger != null && Logger.IsVerbose3)
            {
                Logger.Verbose3($"{ nameof(StatsdClientMetricsProvider)}.ReportStats called with {0} counters, name: {1}, id: {2}", statsCounters.Count, State.SiloName, State.Id);
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
                if (Logger != null && Logger.IsVerbose)
                {
                    Logger.Verbose($"{ nameof(StatsdClientMetricsProvider)}.ReportStats failed: {0}", ex);
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
