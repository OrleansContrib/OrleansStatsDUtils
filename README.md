# OrleansStatsDUtils

Orleans statistics provider for StatsD

## Supports
- [x] Silo statistics
- [x] Client statistics
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
                StatsDServerPort="8125"
                StatsDPrefix="app_name"
                StatsDMaxUdpPacketSize="512"/>
    </StatisticsProviders>
  </Globals>
  <Defaults>
    <Statistics ProviderType="StatsDStatisticsProvider" WriteLogStatisticsToTable="true"/>
  </Defaults>
</OrleansConfiguration>
```
