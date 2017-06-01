using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using StatsdClient;

namespace SBTech.OrleansStatsDUtils
{
    class State
    {
        public string DeploymentId { get; set; } = "";
        public bool IsSilo { get; set; } = true;
        public string SiloName { get; set; } = "";
        public string Id { get; set; } = "";
        public string Address { get; set; } = "";
        public string GatewayAddress { get; set; } = "";
        public string HostName { get; set; } = "";

        public string StatsDServerName { get; set; } = "127.0.0.1";
        public int StatsDServerPort { get; set; } = 8125;
        public string StatsDPrefix { get; set; } = "";
        public int StatsDMaxUdpPacketSize { get; set; } = 512;
    }
}