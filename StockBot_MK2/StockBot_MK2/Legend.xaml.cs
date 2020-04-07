using System;
using System.Collections.Generic;
using System.Linq;
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

namespace StockBot_MK2
{
    /// <summary>
    /// Interaction logic for Legend.xaml
    /// </summary>
    public partial class Legend : UserControl
    {
        private Dictionary<string, TextBlock> _stockTextBlocks;
        private Dictionary<string, Color> _stockTextColors;
        private TextBlock _equityTextBlock;
        private bool _initialized = false;
        private readonly Color _defaultStockTextColor = Colors.Purple;
        private readonly Color _defaultEquityTextColor = Colors.Purple;
        private readonly Color _selectedStockBackgroungColor = Color.FromArgb(80, 128, 128, 128);
        public EventHandler SelectedStockChenged;

        public Legend()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            _stockTextColors = new Dictionary<string, Color>();
            _stockTextBlocks = new Dictionary<string, TextBlock>();
            var i = 0;
            foreach (var stock in Settings.Stocks)
            {
                _stockTextColors.Add(stock, _defaultStockTextColor);
                _stockTextBlocks.Add(stock, new TextBlock
                {
                    Foreground = new SolidColorBrush(_stockTextColors[stock]),
                    Width = 90,
                    Height = 20,
                    Margin = new Thickness(0, 20 * i++, 0, 0),
                });
                _stockTextBlocks[stock].MouseLeftButtonDown += OnMouseLeftButtonDown;
                LegendCanvas.Children.Add(_stockTextBlocks[stock]);
            }

            _equityTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(_defaultEquityTextColor),
                Width = 90,
                Height = 20,
                FontWeight = FontWeights.ExtraBold,
            };
            LegendCanvas.Children.Add(_equityTextBlock);

            _initialized = true;
        }

        public void Update(ref Dictionary<string, List<MarketDataTimePoint>> marketData, AlpacaAccountData accountData, List<AlpacaPosition> positionData)
        {
            if (!_initialized) return;

            foreach (var stock in Settings.Stocks)
            {
                var count = marketData[stock].Count;
                if (count < 1) return;
                if (count > 1)
                {
                    if (Convert.ToDouble(marketData[stock][count - 1].Price()) >
                        Convert.ToDouble(marketData[stock][count - 2].Price()))
                        _stockTextColors[stock] = Colors.Green;
                    else if (Convert.ToDouble(marketData[stock][count - 1].Price()) <
                             Convert.ToDouble(marketData[stock][count - 2].Price()))
                        _stockTextColors[stock] = Colors.Red;
                    else _stockTextColors[stock] = Colors.Gray;
                }

                _stockTextBlocks[stock].Text = "(" + AlpacaData.GetPositionCount(stock) + ") " + stock + " - " +
                                               $"{Convert.ToDouble(marketData[stock][count - 1].Price()):#.00} " +
                                               accountData.Currency();
                _stockTextBlocks[stock].Width = LegendCanvas.ActualWidth;
                _stockTextBlocks[stock].Foreground = new SolidColorBrush(_stockTextColors[stock]);

                if (stock == Settings.SelectedStock)
                {
                    _stockTextBlocks[stock].Background = new SolidColorBrush(_selectedStockBackgroungColor);
                    _stockTextBlocks[stock].FontWeight = FontWeights.ExtraBold;
                }
                else
                {
                    _stockTextBlocks[stock].Background = new SolidColorBrush(Colors.Transparent);
                    _stockTextBlocks[stock].FontWeight = FontWeights.Normal;
                }

                _equityTextBlock.Text = $"Equity: {accountData.Equity():#.00} " + accountData.Currency();
                _equityTextBlock.Margin = new Thickness(0, LegendCanvas.ActualHeight - 20, 0, 0);
                _equityTextBlock.Width = LegendCanvas.ActualWidth;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;
            foreach (var price in _stockTextBlocks)
            {
                if (!price.Value.Equals(tb)) continue;
                Settings.SelectedStock = price.Key;
                break;
            }
            SelectedStockChenged.Invoke(sender, new EventArgs());
            Update(ref MarketData.GetData(), AlpacaData.GetAccountData(), AlpacaData.GetPositionns());
        }

        private void LegendCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update(ref MarketData.GetData(), AlpacaData.GetAccountData(), AlpacaData.GetPositionns());
        }
    }
}
