using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using StatsdClient;

namespace Orleans.Telemetry
{
    public class StatsdTelemetryConsumer :
        IMetricTelemetryConsumer,
        ITraceTelemetryConsumer,
        IEventTelemetryConsumer,
        IExceptionTelemetryConsumer,
        IDependencyTelemetryConsumer,
        IRequestTelemetryConsumer,
        IFlushableLogConsumer
    {
        private readonly string _indexPrefix;

        public StatsdTelemetryConsumer(string indexPrefix = "")
        {
            StatsdConfiguration.CheckConfiguration();

            _indexPrefix = indexPrefix;
        }


        private string Index => _indexPrefix + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH");

        private static string MetricTelemetryType = "metric";
        private const string TraceTelemetryType = "trace";
        private const string EventTelemetryType = "event";
        private const string ExceptionTelemetryType = "exception";
        private const string DependencyTelemetryType = "dependency";
        private const string RequestTelemetryType = "request";
        private const string LogType = "log";

        #region IFlushableLogConsumer

        public void Log(Severity severity, LoggerType loggerType, string caller, string message, IPEndPoint myIPEndPoint, Exception exception, int eventCode = 0)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                tm.Add("Severity", severity.ToString());
                tm.Add("LoggerType", loggerType);
                tm.Add("Caller", caller);
                tm.Add("Message", message);
                tm.Add("IPEndPoint", myIPEndPoint);
                tm.Add("Exception", exception);
                tm.Add("EventCode", eventCode);

                await FinalWrite(tm, LogType);
            });
        }

        #endregion

        #region IExceptionTelemetryConsumer

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                tm.Add("Exception", exception);
                tm.Add("Message", exception.Message);
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

                await FinalWrite(tm, ExceptionTelemetryType);
            });
        }

        #endregion

        #region ITraceTelemetryConsumer

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

        public async Task WriteTrace(string message, Severity severity, IDictionary<string, string> properties)
        {
            var tm = new ExpandoObject() as IDictionary<string, Object>;
            tm.Add("Message", message);
            tm.Add("Severity", severity.ToString());
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    tm.Add(prop.Key, prop.Value);
                }
            }

            await FinalWrite(tm, TraceTelemetryType);
        }


        #endregion

        #region IMetricTelemetryConsumer

        public void DecrementMetric(string name)
        {
            WriteMetric(name, -1, null);
        }

        public void DecrementMetric(string name, double value)
        {
            WriteMetric(name, value * -1, null);
        }

        public void IncrementMetric(string name)
        {
            WriteMetric(name, 1, null);
        }

        public void IncrementMetric(string name, double value)
        {
            WriteMetric(name, value, null);
        }

        public void TrackMetric(string name, TimeSpan value, IDictionary<string, string> properties = null)
        {
            WriteMetric(name, value.TotalMilliseconds, properties);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            WriteMetric(name, value, properties);
        }


        public void WriteMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                //tm.Add("Name", name);
                //tm.Add("Value", value);
                tm.Add(name, value);
                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        tm.Add(prop.Key, prop.Value);
                    }
                }

                await FinalWrite(tm, MetricTelemetryType);
            });
        }


        #endregion

        #region IDependencyTelemetryConsumer

        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                tm.Add("DependencyName", dependencyName);
                tm.Add("CommandName", commandName);
                tm.Add("StartTime", startTime);
                tm.Add("Duration", duration);
                tm.Add("Success", success);

                await FinalWrite(tm, DependencyTelemetryType);
            });
        }

        #endregion

        #region IRequestTelemetryConsumer

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode,
            bool success)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                tm.Add("Request", name);
                tm.Add("StartTime", startTime);
                tm.Add("Duration", duration);
                tm.Add("ResponseCode", responseCode);
                tm.Add("Success", success);

                await FinalWrite(tm, RequestTelemetryType);
            });
        }

        #endregion

        #region IEventTelemetryConsumer
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            Task.Run(async () =>
            {
                var tm = new ExpandoObject() as IDictionary<string, Object>;
                tm.Add("Eventname", eventName);
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

                await FinalWrite(tm, EventTelemetryType);
            });
        }

        #endregion

        #region FinalWrite

        private async Task FinalWrite(IDictionary<string, object> tm, string type)
        {
            tm.Add("UtcDateTime", DateTimeOffset.UtcNow.UtcDateTime);
            tm.Add("MachineName", Environment.MachineName);

            foreach (var item in tm)
            {
                Metrics.Set(item.Key, item.Value.ToString());
            }
        }

        #endregion

        public void Flush()
        {
        }

        public void Close()
        {
        }
    }
}