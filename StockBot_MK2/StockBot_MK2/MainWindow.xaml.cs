using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum TransactionOption
        {
            BUY,
            SELL
        }

        private DispatcherTimer _marketDataUpdater;
        private DispatcherTimer _accountDataUpdater;

        public MainWindow()
        {
            InitializeComponent();

            InitializeMarketData();

            InitializeAccountData();

            InitializePlot();

            InitializeLegend();

            InitializeMarketDataUpdater();

            InitializeAccountDataUpdater();
        }

        private void InitializeMarketData()
        {
            MarketData.Initialize();
            MarketData.LogMessage += LogMessage;
        }

        private void InitializeAccountData()
        {
            AlpacaData.Initialize();
            AlpacaData.LogMessage += LogMessage;
        }

        private void InitializePlot()
        {
            Plot.Initialize();
        }

        private void InitializeLegend()
        {
            Legend.Initialize();

            Legend.SelectedStockChenged += SelectedStockChenged;
        }

        private void SelectedStockChenged(object sender, EventArgs e)
        {
            Plot.Update(ref MarketData.GetData());
        }

        private void InitializeMarketDataUpdater()
        {
            _marketDataUpdater = new DispatcherTimer();
            _marketDataUpdater.Tick += UpdateMarketData;
            _marketDataUpdater.Interval = new TimeSpan(0, 0, 0, Settings.MarketDataRefreshRate, 0);
            _marketDataUpdater.Start();
        }

        private void InitializeAccountDataUpdater()
        {
            _accountDataUpdater = new DispatcherTimer();
            _accountDataUpdater.Tick += UpdateAlpacaData;
            _accountDataUpdater.Interval = new TimeSpan(0, 0, 0, Settings.AlpacaDataRefreshRate, 0);
            _accountDataUpdater.Start();
        }

        private async void UpdateMarketData(object sender, EventArgs e)
        {
            _marketDataUpdater.Stop();
            //await MarketData.Update();
            MarketData.UpdateFake();
            _marketDataUpdater.Start();
            //await AnalizeBuy();
            Plot.Update(ref MarketData.GetData());
            Legend.Update(ref MarketData.GetData(), AlpacaData.GetAccountData(), AlpacaData.GetPositionns());
        }

        private async void UpdateAlpacaData(object sender, EventArgs e)
        {
            _accountDataUpdater.Stop();
            await AlpacaData.Update();
            _accountDataUpdater.Start();
            await AnalizeSell();
            UpdatePositions();
            Legend.Update(ref MarketData.GetData(), AlpacaData.GetAccountData(), AlpacaData.GetPositionns());
        }

        private void UpdatePositions()
        {
            Positions.Clear();
            foreach (var position in AlpacaData.GetPositionns())
                Positions.Text +=
                    $"{position.Symbol()} {position.Volume():#.00} {position.CurrenPrice():#.00}{AlpacaData.GetAccountData().Currency()}" +
                    Environment.NewLine;
            Positions.Foreground = new SolidColorBrush(Colors.Purple);
        }

        private async Task AnalizeBuy()
        {
            foreach (var stock in Settings.Stocks)
            {
                var data = MarketData.GetData()[stock];
                var lastIndex = data.Count - 1;

                if (lastIndex < 2) return;

                var p0 = Convert.ToDouble(data[lastIndex - 0].Price());
                var p1 = Convert.ToDouble(data[lastIndex - 1].Price());
                var p2 = Convert.ToDouble(data[lastIndex - 2].Price());

                var buyVolume = 5;

                if (p0 > p1 && p1 > p2 && p0 - p1 > p1 - p2 && AlpacaData.GetPositionCount(stock) == 0 &&
                    AlpacaData.GetAccountData().Equity() - p0 * buyVolume >= 0)
                    await Buy(stock, p0, buyVolume);

                //if ((p0 < p1 || p0 - p1 < p1 - p2) && AlpacaData.GetPositionCount(stock) > 0)
                //    await Sell(stock, p2);
            }
        }

        private async Task AnalizeSell()
        {
            foreach (var stock in Settings.Stocks)
            {
                if(AlpacaData.GetPositionCount(stock) == 0) continue;

                var stockShortHistory = AlpacaData.GetPositionShortHistory()[stock];

                if (stockShortHistory.Count < 2) return;

                var p0 = stockShortHistory[stockShortHistory.Count - 1];
                var p1 = stockShortHistory[stockShortHistory.Count - 2];

                if (p0 < p1 && AlpacaData.GetPositionCount(stock) > 0)
                    await Sell(stock, p0);
            }
        }

        private async Task Buy(string stock, double price, int volume)
        {
            if (await MakeTransaction(stock, volume, TransactionOption.BUY))
            {
                LogMessage("Buy " + volume + " of " + stock + " at " + price);
            }
            else
            {
                LogMessage("Buy " + volume + " of " + stock + " at " + price + " - FAILED");
            }
        }

        private async Task Sell(string stock, double price)
        {
            var stockCount = AlpacaData.GetPositionCount(stock);
            if (await MakeTransaction(stock, stockCount, TransactionOption.SELL))
            {
                AlpacaData.ClearPositionShortHistory(stock);
                LogMessage("Sell " + stockCount + " of " + stock + " at " + price);
            }
            else
            {
                LogMessage("Sell " + stockCount + " of " + stock + " at " + price + " - FAILED");
            }
        }

        private async Task<bool> MakeTransaction(string stock, int volume, TransactionOption option)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.Method = new HttpMethod("POST");
                request.Headers.Add("APCA-API-KEY-ID", Settings.AlpacaAPIKeyID);
                request.Headers.Add("APCA-API-SECRET-KEY", Settings.AlpacaSecretKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = new JObject(
                    new JProperty("symbol", stock),
                    new JProperty("qty", volume),
                    new JProperty("side", option == TransactionOption.BUY ? "buy" : "sell"),
                    new JProperty("type", "market"),
                    new JProperty("time_in_force", "day")
                );

                request.Content = new StringContent(JsonConvert.SerializeObject(json));
                request.RequestUri =
                    new Uri(Settings.AlpacaEndpoint + "/v2/orders", UriKind.RelativeOrAbsolute);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    dynamic contentJson = JsonConvert.DeserializeObject(contentString);
                }
                else
                {
                    LogMessage("Transaction failed - " + response.ReasonPhrase);
                    return false;
                }
            }
            catch (Exception e)
            {
                LogMessage(e.Message);
                return false;
            }
            return true;
        }

        private void LogMessage(object sender, string e)
        {
            LogMessage(e);
        }

        private void LogMessage(string msg)
        {
            Log.Text += DateTime.Now + " - " + msg + Environment.NewLine;
            Log.Foreground = new SolidColorBrush(Colors.Purple);
            Log.ScrollToEnd();
        }
    }
}
