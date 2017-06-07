using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Orleans.Providers;

namespace Orleans.Telemetry
{
    public class StatsdClientMetricsProvider : StatsdProvider, IConfigurableClientMetricsDataPublisher, IStatisticsPublisher
    {
        internal readonly State State = new State();

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

        public override Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            
            return Task.CompletedTask;
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            State.Id = clientId;
            State.Address = address.ToString();

            return Task.CompletedTask;
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName) => Task.CompletedTask;

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

        static void SendClientPerformanceMetrics(IClientPerformanceMetrics metricsData)
        {
            SendCoreMetrics(metricsData);

            Metrics.GaugeAbsoluteValue("connected_gateway_count", metricsData.ConnectedGatewayCount);
        }

        /// <summary>
        /// Stats for Silo and Client
        /// </summary>  
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
