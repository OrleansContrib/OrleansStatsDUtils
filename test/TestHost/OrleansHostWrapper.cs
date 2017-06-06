using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Telemetry;
using System;

namespace TestHost
{
    internal class OrleansHostWrapper : IDisposable
    {
        public bool Debug
        {
            get { return _siloHost != null && _siloHost.Debug; }
            set { _siloHost.Debug = value; }
        }

        SiloHost _siloHost;

        /// <summary>
        /// start primary
        /// </summary>
        public OrleansHostWrapper()
        {
            var clusterConfig = ClusterConfiguration.LocalhostPrimarySilo();


            //
            // for an easy way to run a ELK stack via docker

            var statsdhost = "";

            StatsdConfiguration.Initialize("localhost", 8125, "test-silo", "test", "local");

            var esTeleM = new StatsdTelemetryConsumer(statsdhost);
            LogManager.TelemetryConsumers.Add(esTeleM);
            LogManager.LogConsumers.Add(esTeleM);


            _siloHost = new SiloHost("primary", clusterConfig);
        }

        public bool Run()
        {
            var ok = false;

            try
            {
                _siloHost.InitializeOrleansSilo();

                ok = _siloHost.StartOrleansSilo();
                if (!ok)
                    throw new SystemException(string.Format("Failed to start Orleans silo '{0}' as a {1} node.",
                        _siloHost.Name, _siloHost.Type));
            }
            catch (Exception exc)
            {
                _siloHost.ReportStartupError(exc);
                var msg = string.Format("{0}:\n{1}\n{2}", exc.GetType().FullName, exc.Message, exc.StackTrace);
                Console.WriteLine(msg);
            }

            return ok;
        }

        public bool Stop()
        {
            var ok = false;

            try
            {
                _siloHost.StopOrleansSilo();
            }
            catch (Exception exc)
            {
                _siloHost.ReportStartupError(exc);
                var msg = $"{exc.GetType().FullName}:\n{exc.Message}\n{exc.StackTrace}";
                Console.WriteLine(msg);
            }

            return ok;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            _siloHost.Dispose();
            _siloHost = null;
        }
    }
}
