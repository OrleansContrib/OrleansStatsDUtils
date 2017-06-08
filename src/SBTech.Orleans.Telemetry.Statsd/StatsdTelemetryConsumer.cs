using Orleans.Runtime;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;

namespace SBTech.Orleans.Telemetry.Statsd
{
    public class StatsdTelemetryConsumer : StatsdProvider, IMetricTelemetryConsumer, ITraceTelemetryConsumer,
                                           IEventTelemetryConsumer, IExceptionTelemetryConsumer,
                                           IDependencyTelemetryConsumer, IRequestTelemetryConsumer, ILogConsumer
    {
        readonly string _indexPrefix;

        public StatsdTelemetryConsumer(string indexPrefix = "")
        {
            _indexPrefix = indexPrefix;
        }

        const string MetricTelemetryType = "metric";
        const string TraceTelemetryType = "trace";
        const string EventTelemetryType = "event";
        const string ExceptionTelemetryType = "exception";
        const string DependencyTelemetryType = "dependency";
        const string RequestTelemetryType = "request";
        const string LogType = "log";

        public void Log(Severity severity, LoggerType loggerType, string caller, string message, IPEndPoint myIPEndPoint,
                        Exception exception, int eventCode = 0)
        {
            var metrics = new ExpandoObject() as IDictionary<string, Object>;

            metrics.Add("severity", severity.ToString());
            metrics.Add("loggerType", loggerType);
            metrics.Add("caller", caller);
            metrics.Add("message", message);
            metrics.Add("ip_end_point", myIPEndPoint);
            metrics.Add("exception", exception);
            metrics.Add("event_code", eventCode);

            FinalWrite(metrics, LogType);
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var tm = new ExpandoObject() as IDictionary<string, Object>;
            tm.Add("exception", exception);
            tm.Add("message", exception.Message);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    tm.Add(prop.Key, prop.Value);
                }
            }
            if (metrics != null)
            {
                foreach (var prop in metrics)
                {
                    tm.Add(prop.Key, prop.Value);
                }
            }

            FinalWrite(tm, ExceptionTelemetryType);
        }

        public void TrackTrace(string message)
        {
            TrackTrace(message, Severity.Info);
        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            if (properties != null)
            {
                TrackTrace(message, Severity.Info, properties);
            }
            else
            {
                TrackTrace(message);
            }
        }

        public void TrackTrace(string message, Severity severity)
        {
            TrackTrace(message, severity, null);
        }

        public void TrackTrace(string message, Severity severity, IDictionary<string, string> properties)
        {
            WriteTrace(message, severity, properties);
        }

        public void DecrementMetric(string name)
        {
            WriteMetric(name, -1);
        }

        public void DecrementMetric(string name, double value)
        {
            WriteMetric(name, value * -1);
        }

        public void IncrementMetric(string name)
        {
            WriteMetric(name, 1);
        }

        public void IncrementMetric(string name, double value)
        {
            WriteMetric(name, value);
        }

        public void TrackMetric(string name, TimeSpan value, IDictionary<string, string> properties = null)
        {
            WriteMetric(name, value.TotalMilliseconds, properties);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            WriteMetric(name, value, properties);
        }

        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            var metrics = new Dictionary<string, Object>();

            metrics.Add("dependency_name", dependencyName);
            metrics.Add("command_name", commandName);
            metrics.Add("start_time", startTime);
            metrics.Add("duration", duration);
            metrics.Add("success", success);

            FinalWrite(metrics, DependencyTelemetryType);
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            var metrics = new Dictionary<string, Object>();

            metrics.Add("request", name);
            metrics.Add("start_time", startTime);
            metrics.Add("duration", duration);
            metrics.Add("response_code", responseCode);
            metrics.Add("success", success);

            FinalWrite(metrics, RequestTelemetryType);
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var eventData = new Dictionary<string, object> { { "event_name", eventName } };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    eventData.Add(prop.Key, prop.Value);
                }
            }
            if (metrics != null)
            {
                foreach (var prop in metrics)
                {
                    eventData.Add(prop.Key, prop.Value);
                }
            }

            FinalWrite(eventData, EventTelemetryType);
        }

        public void Flush()
        {
        }

        void ITelemetryConsumer.Close()
        {
        }

        void FinalWrite(IDictionary<string, object> metrics, string eventType)
        {
            metrics.Add($"utc_date_time_{eventType}", DateTimeOffset.UtcNow.UtcDateTime);
            metrics.Add($"machine_name_{eventType}", Environment.MachineName);

            foreach (var item in metrics)
            {
                Metrics.Set($"{_indexPrefix}.{item.Key}", item.Value?.ToString());
            }
        }

        void WriteTrace(string message, Severity severity, IDictionary<string, string> properties)
        {
            var metrics = new Dictionary<string, Object>();

            metrics.Add("message", message);
            metrics.Add("severity", severity.ToString());

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metrics.Add(prop.Key, prop.Value);
                }
            }

            FinalWrite(metrics, TraceTelemetryType);
        }

        void WriteMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            var metrics = new Dictionary<string, Object>();

            metrics.Add(name, value);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metrics.Add(prop.Key, prop.Value);
                }
            }

            FinalWrite(metrics, MetricTelemetryType);

        }
    }
}