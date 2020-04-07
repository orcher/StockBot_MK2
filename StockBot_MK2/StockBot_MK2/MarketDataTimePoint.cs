using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    public class MarketDataTimePoint
    {
        private JObject _data;

        public MarketDataTimePoint(JObject data)
        {
            _data = data;
        }

        public string Symbol()
        {
            return _data["Global Quote"]["01. symbol"].Value<string>();
        }

        public double Open()
        {
            return _data["Global Quote"]["02. open"].Value<double>();
        }

        public double High()
        {
            return _data["Global Quote"]["03. high"].Value<double>();
        }

        public double Low()
        {
            return _data["Global Quote"]["04. low"].Value<double>();
        }

        public double Price()
        {
            return _data["Global Quote"]["05. price"].Value<double>();
        }

        public double PreviousClose()
        {
            return _data["Global Quote"]["08. previous close"].Value<double>();
        }

    }
}
