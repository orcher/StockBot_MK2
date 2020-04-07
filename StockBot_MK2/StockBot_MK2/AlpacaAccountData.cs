using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StockBot_MK2
{
    public class AlpacaAccountData
    {
        private JObject _data;

        public AlpacaAccountData(JObject data)
        {
            _data = data;
        }

        public double Equity()
        {
            return _data["equity"].Value<double>();
        }

        public string Currency()
        {
            return _data["currency"].Value<string>();
        }
    }
}
