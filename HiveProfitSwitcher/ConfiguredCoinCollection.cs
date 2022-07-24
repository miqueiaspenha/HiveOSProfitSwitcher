using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{
    public class ConfiguredCoinCollection : ConfigurationElementCollection
    {
        public ConfiguredCoinElement this[int index]
        {
            get
            {
                return (ConfiguredCoinElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new ConfiguredCoinElement this[string key]
        {
            get
            {
                return (ConfiguredCoinElement)BaseGet(key);
            }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfiguredCoinElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConfiguredCoinElement)element).CoinTicker;
        }
    }
}
