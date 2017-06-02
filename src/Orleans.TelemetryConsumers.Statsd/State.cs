using System;

namespace Orleans.Telemetry
{
    internal class State
    {
        public string DeploymentId { get; set; }
        public bool IsSilo { get; set; }
        public string SiloName { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public string GatewayAddress { get; set; }
        public string HostName { get; set; }
        public Guid ServiceId { get; set; }

        public State()
        {
            ServiceId = Guid.Empty;
            DeploymentId = string.Empty;
            IsSilo = true;
            SiloName = string.Empty;
            Id = string.Empty;
            Address = string.Empty;
            GatewayAddress = string.Empty;
            HostName = string.Empty;
        }
    }
}