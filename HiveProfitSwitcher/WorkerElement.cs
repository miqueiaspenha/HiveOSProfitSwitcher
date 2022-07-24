using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace HiveProfitSwitcher
{
    public class WorkerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("whatToMineEndpoint", IsKey = false, IsRequired = true)]
        public string WTMEndpoint
        {
            get
            {
                return (string)base["whatToMineEndpoint"];
            }
            set
            {
                base["whatToMineEndpoint"] = value;
            }
        }

        [ConfigurationProperty("enabledCoins")]
        [ConfigurationCollection(typeof(ConfiguredCoinCollection))]
        public ConfiguredCoinCollection EnabledCoins
        {
            get
            {
                return (ConfiguredCoinCollection)this["enabledCoins"];
            }
        }
    }
}
