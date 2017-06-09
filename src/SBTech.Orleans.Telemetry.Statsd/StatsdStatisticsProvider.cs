using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Orleans.Runtime;
using StatsdClient;

namespace SBTech.Orleans.Telemetry.Statsd
{
    public class StatsdStatisticsProvider : StatsdProvider, IConfigurableSiloMetricsDataPublisher,
                                            IStatisticsPublisher
    {
        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName,
            IPEndPoint gateway, string hostName)
        {

            State.Address = siloAddress.Endpoint.ToString();
            State.DeploymentId = deploymentId;
            State.SiloName = siloName;
            State.GatewayAddress = gateway.ToString();
            State.HostName = hostName;

            return Task.CompletedTask;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            State.DeploymentId = deploymentId;
            State.IsSilo = isSilo;
            State.SiloName = siloName;
            State.Address = address;
            State.HostName = hostName;

            return Task.CompletedTask;
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            State.DeploymentId = deploymentId;
            State.IsSilo = isSilo;
            State.SiloName = siloName;
            State.Address = address.ToString();
            State.GatewayAddress = gateway.ToString();
            State.HostName = hostName;
        }

        /// <summary>
        /// Metrics for Silo
        /// </summary>        
        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            try
            {
                SendSiloMetrics(metricsData);
            }
            catch (Exception ex)
            {
                Trace.Write($"{ nameof(StatsdStatisticsProvider)}.ReportMetrics failed: {ex}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
        public Task ReportStats(List<ICounter> statsCounters)
        {
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
                Trace.Write($"{nameof(StatsdStatisticsProvider)}.ReportStats failed: {ex}");            
            }
            return Task.CompletedTask;
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

        static void SendSiloMetrics(ISiloPerformanceMetrics metricsData)
        {
            Metrics.GaugeAbsoluteValue("activations_count", metricsData.ActivationCount);
            Metrics.GaugeAbsoluteValue("recently_used_activations", metricsData.RecentlyUsedActivationCount);
            Metrics.GaugeAbsoluteValue("request_queue_length", metricsData.RequestQueueLength);
            Metrics.GaugeAbsoluteValue("is_overloaded", metricsData.IsOverloaded ? 1 : 0);
            Metrics.GaugeAbsoluteValue("client_count", metricsData.ClientCount);

            SendCoreMetrics(metricsData);
        }
    }
}
