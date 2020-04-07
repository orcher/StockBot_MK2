using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBot_MK2
{
    public class Settings
    {
        public static int MaxSamples = 300;
        public static string AlphaVentageEndpoint = "https://www.alphavantage.co/query";
        public static string AlphaVentageApiKey = "PRQYP9H6RW77UULI";
        public static string AlpacaEndpoint = "https://paper-api.alpaca.markets";
        public static string AlpacaAPIKeyID = "PK5ESABNHUF0NLFDR1ND";
        public static string AlpacaSecretKey = "KtWnDfQ8Qp0T8MfNMorNe7KeS0svofP7YQEy2De8";
        public static List<string> Stocks = new List<string> {"MSFT", "AAPL", "IBM", "EBAY", "FB" };
        public static int MarketDataRefreshRate = 2;
        public static int AlpacaDataRefreshRate = 1;
        public static string SelectedStock = Stocks[0];
    }
}
