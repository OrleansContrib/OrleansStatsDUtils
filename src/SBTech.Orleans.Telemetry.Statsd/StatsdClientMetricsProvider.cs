using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace SBTech.Orleans.Telemetry.Statsd
{
    public class StatsdClientMetricsProvider : StatsdProvider, IConfigurableClientMetricsDataPublisher,
                                               IConfigurableStatisticsPublisher, IConfigurableSiloMetricsDataPublisher
    {
        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            State.DeploymentId = deploymentId;
            State.Id = clientId;
            State.Address = address.ToString();
            State.HostName = hostName;
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            State.DeploymentId = deploymentId;
            State.Address = address.ToString();
            State.HostName = hostName;
            State.SiloName = siloName;
            State.GatewayAddress = gateway.Address.ToString();
            State.IsSilo = isSilo;
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            State.Id = clientId;
            State.Address = address.ToString();

            return Task.CompletedTask;
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            State.DeploymentId = deploymentId;
            State.HostName = hostName;
            State.SiloName = siloName;

            return Task.CompletedTask;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            State.DeploymentId = deploymentId;
            State.Address = address;
            State.HostName = hostName;
            State.SiloName = siloName;
            State.IsSilo = isSilo;

            return Task.CompletedTask;
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            Trace.Write($"{nameof(StatsdClientMetricsProvider)}.ReportMetrics called with metrics: {metricsData}, name: {State.SiloName}, id: {State.Id}.");

            try
            {
                SendCoreMetrics(metricsData);

                Metrics.GaugeAbsoluteValue("request_queue_length     ", metricsData.RequestQueueLength);
                Metrics.GaugeAbsoluteValue("activation_count ", metricsData.ActivationCount);
                Metrics.GaugeAbsoluteValue("recently_used_activation_count ", metricsData.RecentlyUsedActivationCount);
                Metrics.GaugeAbsoluteValue("client_count ", metricsData.ClientCount);
                Metrics.GaugeAbsoluteValue("is_overloaded ", Convert.ToInt32(metricsData.IsOverloaded));
            }
            catch (Exception ex)
            {
                Trace.Write($"{ nameof(StatsdClientMetricsProvider)}.ReportMetrics failed: {ex}");
                throw;
            }

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Metrics for client
        /// </summary>        
        public Task ReportMetrics(IClientPerformanceMetrics metricsData)
        {
            Trace.Write($"{nameof(StatsdClientMetricsProvider)}.ReportMetrics called with metrics: {metricsData}, name: {State.SiloName}, id: {State.Id}.");

            try
            {
                SendClientPerformanceMetrics(metricsData);
            }
            catch (Exception ex)
            {
                Trace.Write($"{ nameof(StatsdClientMetricsProvider)}.ReportMetrics failed: {ex}");
                throw;
            }

            return Task.CompletedTask;
        }

        public Task ReportStats(List<ICounter> statsCounters)
        {
            Trace.Write($"{ nameof(StatsdClientMetricsProvider)}.ReportStats called with {statsCounters.Count} counters, name: {State.SiloName}, id: , {State.Id}");

            try
            {
                foreach (var counter in statsCounters)
                {
                    SendStats(counter);
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
                throw;
            }

            return Task.CompletedTask;
        }

        static void SendClientPerformanceMetrics(IClientPerformanceMetrics metricsData)
        {
            SendCoreMetrics(metricsData);

            Metrics.GaugeAbsoluteValue("connected_gateway_count", metricsData.ConnectedGatewayCount);
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
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
