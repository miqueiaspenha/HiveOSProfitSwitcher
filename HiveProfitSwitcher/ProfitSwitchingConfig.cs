﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{

    public class ProfitSwitchingConfig : ConfigurationSection
    {
        [ConfigurationProperty("workers")]
        [ConfigurationCollection(typeof(WorkersCollection))]
        public WorkersCollection Workers
        {
            get
            {
                return (WorkersCollection)this["workers"];
            }
        }
    }
}
