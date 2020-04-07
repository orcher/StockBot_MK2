using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    public class AlpacaPosition
    {
        private JObject _data;

        public AlpacaPosition(JObject data)
        {
            _data = data;
        }

        public string Symbol()
        {
            return _data["symbol"].Value<string>();
        }

        public int Volume()
        {
            return _data["qty"].Value<int>();
        }

        public double  MarketValue()
        {
            return _data["market_value"].Value<double>();
        }

        public double CurrenPrice()
        {
            return _data["current_price"].Value<double>();
        }

        public double ChangeToday()
        {
            return _data["change_today"].Value<double>();
        }

        public double Unrealized_plpc()
        {
            return _data["unrealized_plpc"].Value<double>();
        }
    }
}
