using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Orleans.Runtime.Configuration;

namespace Orleans.Telemetry
{
    public static class ProviderConfigurationExtensions
    {
        public static void AddStatsdStatisticsProvider(this ClusterConfiguration config,
            string providerName, Uri statsdHostAddress, int statsdPort, string statsdPrefix = "orleans_statistics", CultureInfo statsdCultureInfo = null)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrWhiteSpace(statsdPrefix)) throw new ArgumentNullException(nameof(statsdPrefix));

            statsdCultureInfo = statsdCultureInfo ?? Thread.CurrentThread.CurrentCulture;

            var properties = new Dictionary<string, string>
            {
                {StatsdProvider.StatsdHostAddress, statsdHostAddress.ToString()},
                {StatsdProvider.StatsdPort, statsdPort.ToString()},
                {StatsdProvider.StatsdPrefix , statsdPrefix},
                {StatsdProvider.StatsdCultureInfo, statsdCultureInfo.ToString()},
            };

            config.Globals.RegisterStatisticsProvider<StatsdStatisticsProvider>(providerName, properties);
        }

    }
}
