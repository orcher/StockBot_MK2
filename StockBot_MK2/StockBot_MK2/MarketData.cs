using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    public static class MarketData
    {
        private static Dictionary<string, List<MarketDataTimePoint>> _inrtaDayData;
        private static bool _initialized = false;
        public static EventHandler<string> LogMessage;

        public static void Initialize()
        {
            _inrtaDayData = new Dictionary<string, List<MarketDataTimePoint>>();
            foreach (var stock in Settings.Stocks) _inrtaDayData[stock] = new List<MarketDataTimePoint>();

            _initialized = true;
        }

        public static async Task<bool> Update()
        {
            if (!_initialized) return false;

            LimitDataStoring();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (var stock in Settings.Stocks)
            {
                try
                {
                    var url = Settings.AlphaVentageEndpoint + "?function=GLOBAL_QUOTE&symbol=" + stock + "&apikey=" + Settings.AlphaVentageApiKey;
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentString = await response.Content.ReadAsStringAsync();
                        dynamic contentJson = JsonConvert.DeserializeObject(contentString);
                        if (contentString.Contains("Global Quote"))
                        {
                            _inrtaDayData[stock].Add(new MarketDataTimePoint((JObject)contentJson));
                        }
                        else if(contentString.Contains("Note"))
                        {
                            LogMessage.Invoke(null, ((JObject)contentJson)["Note"].Value<string>());
                            return false;
                        }
                        else
                        {
                            LogMessage.Invoke(null, contentString);
                            return false;
                        }
                    }
                    else
                    {
                        LogMessage.Invoke(null, response.ReasonPhrase);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    LogMessage.Invoke(null, e.Message);
                    return false;
                }
            }

            return true;
        }

        public static void UpdateFake()
        {
            LimitDataStoring();

            var rnd = new Random();
            foreach (var stock in Settings.Stocks)
            {
                var obj = new JObject
                {
                    new JProperty("Global Quote", new JObject
                    {
                        new JProperty("01. symbol", "0"),
                        new JProperty("02. open", 0),
                        new JProperty("03. high", 2000),
                        new JProperty("04. low", 0),
                        new JProperty("05. price", rnd.NextDouble() * 2000),
                        new JProperty("06. volume", 0),
                        new JProperty("07. latest trading day", 0),
                        new JProperty("08. previous close", 0),
                        new JProperty("09. change", 0),
                        new JProperty("10. change percent", 0),
                    })
                };
                _inrtaDayData[stock].Add(new MarketDataTimePoint(obj));
            }
        }

        public static ref Dictionary<string, List<MarketDataTimePoint>> GetData()
        {
            return ref _inrtaDayData;
        }

        private static void LimitDataStoring()
        {
            foreach (var stock in Settings.Stocks)
                if (_inrtaDayData[stock].Count > Settings.MaxSamples)
                    _inrtaDayData[stock].RemoveAt(0);
        }

    }
}
