using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{
    public  class ConfiguredCoinElement : ConfigurationElement
    {
        [ConfigurationProperty("coinTicker", IsKey = true, IsRequired = true)]
        public string CoinTicker
        {
            get
            {
                return (string)base["coinTicker"];
            }
            set
            {
                base["coinTicker"] = value;
            }
        }

        [ConfigurationProperty("flightSheetName", IsKey = false, IsRequired = true)]
        public string FlightSheetName
        {
            get
            {
                return (string)base["flightSheetName"];
            }
            set
            {
                base["flightSheetName"] = value;
            }
        }
    }
}
