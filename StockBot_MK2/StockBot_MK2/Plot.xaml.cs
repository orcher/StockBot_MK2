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
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    /// <summary>
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : UserControl
    {
        private Dictionary<string, Polyline> _stockValueLines;
        private Dictionary<string, Color> _stockValueLineColors;
        private readonly Color _defaultStockValueLineColor = Colors.Purple;

        private bool _initialized = false;

        public Plot()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            _stockValueLineColors = new Dictionary<string, Color>();
            _stockValueLines = new Dictionary<string, Polyline>();
            foreach (var stock in Settings.Stocks)
            {
                _stockValueLineColors.Add(stock, _defaultStockValueLineColor);

                _stockValueLines.Add(stock, new Polyline
                {
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(_stockValueLineColors[stock])
                });

                PlotCanvas.Children.Add(_stockValueLines[stock]);
            }

            _initialized = true;
        }

        public void Update(ref Dictionary<string, List<MarketDataTimePoint>> marketData)
        {
            if (!_initialized) return;

            foreach (var stockData in marketData)
            {
                var stock = stockData.Key;
                var data = stockData.Value;
                if (stock != Settings.SelectedStock)
                {
                    _stockValueLines[stock].Stroke = new SolidColorBrush(Colors.Transparent);
                    continue;
                }

                var pc = new PointCollection();
                var samplesCount = data.Count;
                for (var i = 0; i < samplesCount; i++)
                {
                    var price = Convert.ToDouble(data[i].Price());
                    var high = Convert.ToDouble(data[i].High());
                    var low = Convert.ToDouble(data[i].Low());
                    var ratio = PlotCanvas.ActualHeight / (high - low);
                    var x = i * (PlotCanvas.ActualWidth / Settings.MaxSamples);
                    var y = PlotCanvas.ActualHeight - (price - low) * ratio;
                    pc.Add(new Point(x, y));
                }
                _stockValueLines[stock].Points = pc;
                _stockValueLines[stock].Stroke = new SolidColorBrush(_stockValueLineColors[stock]);
            }
        }

        private void PlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update(ref MarketData.GetData());
        }
    }
}
