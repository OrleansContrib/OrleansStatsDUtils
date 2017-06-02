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
            throw new NotImplementedException();
        }

    }
}
