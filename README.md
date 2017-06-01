# Orleans.TelemetryConsumers.Statsd

A collection of telemtry consumers delivering data to Statsd.

## Usage

* get your statsd  settings
* choose an index prefix


```cs
StatsdConfiguration.Initialize(serverName, port,  prefix, siloName, hostName, maxUdpPacketSize);

var telem = new StatsdTelemetryConsumer("orleans_telemetry");
LogManager.TelemetryConsumers.Add(telem);
LogManager.LogConsumers.Add(telem);

///



//then start your silo
siloHost = new SiloHost("primary", clusterConfig);
```

## Supports
- [x] Silo statistics
- [ ] Client statistics
- [ ] .NET Core

## Configuration for Silo
```xml
<?xml version="1.0" encoding="utf-8" ?>
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
    <StatisticsProviders>
      <Provider Type="Orleans.TelemetryConsumers.Statsd.StatsDStatisticsProvider"
                Name="StatsdStatisticsProvider"
                StatsDServerName="localhost"
                StatsDPrefix="app_name" />
    </StatisticsProviders>
  </Globals>
  <Defaults>
    <Statistics ProviderType="StatsDStatisticsProvider" WriteLogStatisticsToTable="true"/>
  </Defaults>
</OrleansConfiguration>
```

## Environmental setup

You need an ElasticSearch host, and likely you want Kibana to view the data

### start your silo(s)

see https://gitter.im/dotnet/orleans or create an issue here for problems
