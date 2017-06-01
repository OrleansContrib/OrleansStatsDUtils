﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Telemetry;

namespace TestHost
{
    internal class OrleansHostWrapper : IDisposable
    {
        public bool Debug
        {
            get { return siloHost != null && siloHost.Debug; }
            set { siloHost.Debug = value; }
        }

        private SiloHost siloHost;

        /// <summary>
        /// start primary
        /// </summary>
        public OrleansHostWrapper()
        {
            var clusterConfig = ClusterConfiguration.LocalhostPrimarySilo();


            //
            // for an easy way to run a ELK stack via docker

            var statsdhost = "";

            var esTeleM = new StatsdTelemetryConsumer(statsdhost);
            LogManager.TelemetryConsumers.Add(esTeleM);
            LogManager.LogConsumers.Add(esTeleM);


            siloHost = new SiloHost("primary", clusterConfig);
        }



        public bool Run()
        {
            var ok = false;

            try
            {
                siloHost.InitializeOrleansSilo();

                ok = siloHost.StartOrleansSilo();
                if (!ok)
                    throw new SystemException(string.Format("Failed to start Orleans silo '{0}' as a {1} node.",
                        siloHost.Name, siloHost.Type));
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
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
                siloHost.StopOrleansSilo();
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
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
            siloHost.Dispose();
            siloHost = null;
        }
    }
}
