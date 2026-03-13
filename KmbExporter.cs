using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using KmbTextConversion;

namespace KmbRouteDownloader
{
    internal sealed class KmbExporter
    {
        private const string BaseUrl = "https://data.etabus.gov.hk/v1/transport/kmb/";

        private readonly Action<string> _log;
        private readonly JavaScriptSerializer _serializer;
        private readonly IChineseScriptConverter _scriptConverter;

        public KmbExporter(Action<string> log)
        {
            _log = log ?? (_ => { });
            _serializer = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };
            _scriptConverter = new ChineseScriptConverter();
        }

        public async Task<ExportResult> ExportAsync(string outputPath)
        {
            Directory.CreateDirectory(Path.GetFullPath(outputPath));

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);

                List<RouteRecord> routes = await DownloadRoutesAsync(client);
                List<StopRecord> stops = await DownloadStopsAsync(client);
                List<RouteStopRecord> routeStops = await DownloadRouteStopsAsync(client);

                _log(string.Format(CultureInfo.InvariantCulture, "Routes: {0}, Stops: {1}, Route-stops: {2}", routes.Count, stops.Count, routeStops.Count));

                var routeLookup = routes
                    .GroupBy(route => RouteKey.Create(route.Route, route.Bound, route.ServiceType), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

                var stopLookup = stops
                    .GroupBy(stop => stop.Stop, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

                List<RouteCsvRow> rows = routeStops
                    .Where(routeStop => routeLookup.ContainsKey(RouteKey.Create(routeStop.Route, routeStop.Bound, routeStop.ServiceType)))
                    .Where(routeStop => stopLookup.ContainsKey(routeStop.Stop))
                    .OrderBy(routeStop => routeStop.Route, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(routeStop => routeStop.Bound, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(routeStop => ParseServiceType(routeStop.ServiceType))
                    .ThenBy(routeStop => routeStop.Sequence)
                    .Select(routeStop =>
                    {
                        RouteRecord route = routeLookup[RouteKey.Create(routeStop.Route, routeStop.Bound, routeStop.ServiceType)];
                        StopRecord stop = stopLookup[routeStop.Stop];
                        return new RouteCsvRow
                        {
                            BusNumber = routeStop.Route,
                            StopName = stop.NameTc,
                            Direction = PrefixDirection(route.DestinationTc),
                            StopId = stop.Stop,
                            StopNameSc = ConvertToSimplified(stop.NameTc),
                            DirectionSc = ConvertToSimplified(PrefixDirection(route.DestinationTc))
                        };
                    })
                    .ToList();

                int fileCount = WriteRouteFiles(outputPath, rows);
                return new ExportResult
                {
                    RowCount = rows.Count,
                    FileCount = fileCount
                };
            }
        }

        private async Task<List<RouteRecord>> DownloadRoutesAsync(HttpClient client)
        {
            IList items = await DownloadDataAsync(client, "route");
            return items
                .OfType<Dictionary<string, object>>()
                .Select(item => new RouteRecord
                {
                    Route = ReadString(item, "route"),
                    Bound = ReadString(item, "bound"),
                    ServiceType = ReadString(item, "service_type"),
                    DestinationTc = ReadString(item, "dest_tc")
                })
                .Where(route => !string.IsNullOrWhiteSpace(route.Route))
                .ToList();
        }

        private async Task<List<StopRecord>> DownloadStopsAsync(HttpClient client)
        {
            IList items = await DownloadDataAsync(client, "stop");
            return items
                .OfType<Dictionary<string, object>>()
                .Select(item => new StopRecord
                {
                    Stop = ReadString(item, "stop"),
                    NameTc = ReadString(item, "name_tc")
                })
                .Where(stop => !string.IsNullOrWhiteSpace(stop.Stop))
                .ToList();
        }

        private async Task<List<RouteStopRecord>> DownloadRouteStopsAsync(HttpClient client)
        {
            IList items = await DownloadDataAsync(client, "route-stop");
            return items
                .OfType<Dictionary<string, object>>()
                .Select(item => new RouteStopRecord
                {
                    Route = ReadString(item, "route"),
                    Bound = ReadString(item, "bound"),
                    ServiceType = ReadString(item, "service_type"),
                    Sequence = ReadInt(item, "seq"),
                    Stop = ReadString(item, "stop")
                })
                .Where(routeStop => !string.IsNullOrWhiteSpace(routeStop.Route) && !string.IsNullOrWhiteSpace(routeStop.Stop))
                .ToList();
        }

        private async Task<IList> DownloadDataAsync(HttpClient client, string endpoint)
        {
            string url = BaseUrl + endpoint + "/";
            _log("GET " + url);

            string json = await client.GetStringAsync(url).ConfigureAwait(false);
            var envelope = _serializer.DeserializeObject(json) as Dictionary<string, object>;
            if (envelope == null || !envelope.ContainsKey("data"))
            {
                throw new InvalidOperationException("Unexpected response from KMB API: missing data array.");
            }

            var items = envelope["data"] as IList;
            if (items == null)
            {
                throw new InvalidOperationException("Unexpected response from KMB API: data is not an array.");
            }

            return items;
        }

        private static string ReadString(IDictionary<string, object> item, string key)
        {
            object value;
            return item.TryGetValue(key, out value) ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;
        }

        private string ConvertToSimplified(string traditionalText)
        {
            return _scriptConverter.ToSimplified(traditionalText);
        }

        private static int ReadInt(IDictionary<string, object> item, string key)
        {
            object value;
            int parsed;
            return item.TryGetValue(key, out value) && int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
                ? parsed
                : 0;
        }

        private static string PrefixDirection(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return string.Empty;
            }

            return destination.StartsWith("\u5F80", StringComparison.Ordinal) ? destination : "\u5F80" + destination;
        }

        private static int ParseServiceType(string serviceType)
        {
            int parsed;
            return int.TryParse(serviceType, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : int.MaxValue;
        }

        private static int WriteRouteFiles(string outputFolder, IEnumerable<RouteCsvRow> rows)
        {
            int fileCount = 0;
            foreach (IGrouping<string, RouteCsvRow> routeGroup in rows.GroupBy(row => row.BusNumber, StringComparer.OrdinalIgnoreCase))
            {
                string safeRouteName = MakeSafeFileName(routeGroup.Key);
                string outputPath = Path.Combine(outputFolder, safeRouteName + ".csv");
                using (var writer = new StreamWriter(outputPath, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("bus_number,stop_name,direction,stop_id,stop_name_sc,direction_sc");

                    foreach (RouteCsvRow row in routeGroup)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            EscapeCsv(row.BusNumber),
                            EscapeCsv(row.StopName),
                            EscapeCsv(row.Direction),
                            EscapeCsv(row.StopId),
                            EscapeCsv(row.StopNameSc),
                            EscapeCsv(row.DirectionSc)
                        }));
                    }
                }

                fileCount++;
            }

            return fileCount;
        }

        private static string EscapeCsv(string value)
        {
            string text = value ?? string.Empty;
            bool needsQuotes = text.Contains(",") || text.Contains("\"") || text.Contains("\r") || text.Contains("\n");
            text = text.Replace("\"", "\"\"");
            return needsQuotes ? "\"" + text + "\"" : text;
        }

        private static string MakeSafeFileName(string routeNumber)
        {
            string sanitized = routeNumber ?? string.Empty;
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalidChar.ToString(), "_");
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "unknown-route" : sanitized;
        }

        private static class RouteKey
        {
            public static string Create(string route, string bound, string serviceType)
            {
                return (route ?? string.Empty) + "|" + (bound ?? string.Empty) + "|" + (serviceType ?? string.Empty);
            }
        }
    }

    internal sealed class RouteRecord
    {
        public string Route { get; set; }
        public string Bound { get; set; }
        public string ServiceType { get; set; }
        public string DestinationTc { get; set; }
    }

    internal sealed class StopRecord
    {
        public string Stop { get; set; }
        public string NameTc { get; set; }
    }

    internal sealed class RouteStopRecord
    {
        public string Route { get; set; }
        public string Bound { get; set; }
        public string ServiceType { get; set; }
        public int Sequence { get; set; }
        public string Stop { get; set; }
    }

    internal sealed class RouteCsvRow
    {
        public string BusNumber { get; set; }
        public string StopName { get; set; }
        public string Direction { get; set; }
        public string StopId { get; set; }
        public string StopNameSc { get; set; }
        public string DirectionSc { get; set; }
    }

    internal sealed class ExportResult
    {
        public int RowCount { get; set; }
        public int FileCount { get; set; }
    }
}
