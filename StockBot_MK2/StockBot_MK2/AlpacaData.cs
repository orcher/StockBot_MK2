using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    public static class AlpacaData
    {
        private static AlpacaAccountData _accountData;
        private static List<AlpacaPosition> _positionsData;
        private static Dictionary<string, List<double>> _positionShortHistory;
        private static bool _initialized = false;
        public static EventHandler<string> LogMessage;

        public static void Initialize()
        {
            _accountData = new AlpacaAccountData(new JObject());
            _positionsData = new List<AlpacaPosition>();
            _positionShortHistory = new Dictionary<string, List<double>>();
            foreach (var stock in Settings.Stocks) _positionShortHistory[stock] = new List<double>();

            _initialized = true;
        }

        public static async Task<bool> Update()
        {
            var ret = true;
            ret &= await UpdateAccountData();
            ret &= await UpdatePositionsData();
            if (!ret) Console.WriteLine("Updating account data failed");
            return ret;
        }

        public static ref AlpacaAccountData GetAccountData()
        {
            return ref _accountData;
        }

        public static ref List<AlpacaPosition> GetPositionns()
        {
            return ref _positionsData;
        }

        public static int GetPositionCount(string stock)
        {
            foreach (var position in _positionsData)
                if (position.Symbol() == stock) return position.Volume();

            return 0;
        }

        public static ref Dictionary<string, List<double>> GetPositionShortHistory()
        {
            return ref _positionShortHistory;
        }

        public static void ClearPositionShortHistory(string stock)
        {
            _positionShortHistory[stock].Clear();
        }

        private static async Task<bool> UpdateAccountData()
        {
            if (!_initialized) return false;

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.Method = new HttpMethod("GET");
                request.Headers.Add("APCA-API-KEY-ID", Settings.AlpacaAPIKeyID);
                request.Headers.Add("APCA-API-SECRET-KEY", Settings.AlpacaSecretKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.RequestUri = new Uri(Settings.AlpacaEndpoint + "/v2/account", UriKind.RelativeOrAbsolute);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    LogMessage.Invoke(null, response.ReasonPhrase);
                    return false;
                }
                var contentString = await response.Content.ReadAsStringAsync();
                dynamic contentJson = JsonConvert.DeserializeObject(contentString);
                _accountData = new AlpacaAccountData((JObject)contentJson);
            }
            catch (Exception e)
            {
                LogMessage.Invoke(null, e.Message);
                return false;
            }
            
            return true;
        }

        private static async Task<bool> UpdatePositionsData()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.Method = new HttpMethod("GET");
                request.Headers.Add("APCA-API-KEY-ID", Settings.AlpacaAPIKeyID);
                request.Headers.Add("APCA-API-SECRET-KEY", Settings.AlpacaSecretKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                request.RequestUri = new Uri(Settings.AlpacaEndpoint + "/v2/positions", UriKind.RelativeOrAbsolute);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    LogMessage.Invoke(null, response.ReasonPhrase);
                    return false;
                }
                var contentString = await response.Content.ReadAsStringAsync();
                dynamic contentJson = JsonConvert.DeserializeObject(contentString);
                _positionsData.Clear();
                var positions = (JArray)contentJson;
                foreach (var position in positions)
                {
                    _positionsData.Add(new AlpacaPosition((JObject)position));
                    _positionShortHistory[_positionsData[_positionsData.Count-1].Symbol()].Add(_positionsData[_positionsData.Count - 1].CurrenPrice());
                }
            }
            catch (Exception e)
            {
                LogMessage.Invoke(null, e.Message);
                return false;
            }
            
            return true;
        }

    }
}
