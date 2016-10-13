# OrleansStatsDUtils
Orleans statistics provider for StatsD

## How to install
To install OrleansElasticUtils via NuGet, run this command in NuGet package manager console:
```code
PM> Install-Package SBTech.OrleansStatsDUtils
```

**Supports:**
- [x] Silo statistics
- [ ] Client statistics
- [ ] .NET Core

**Configuration for Silo**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
    <StatisticsProviders>
      <Provider Type="SBTech.OrleansStatsDUtils.StatsDStatisticsProvider"
                Name="StatsDStatisticsProvider"
                StatsDServerName="localhost"
                StatsDPrefix="app_name" />
    </StatisticsProviders>
  </Globals>
  <Defaults>
    <Statistics ProviderType="StatsDStatisticsProvider" WriteLogStatisticsToTable="true"/>
  </Defaults>
</OrleansConfiguration>
```
