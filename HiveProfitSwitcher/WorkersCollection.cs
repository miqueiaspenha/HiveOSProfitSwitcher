using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{
    public class WorkersCollection : ConfigurationElementCollection
    {
        public WorkerElement this[int index]
        {
            get
            {
                return (WorkerElement)BaseGet(index);
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

        public new WorkerElement this[string key]
        {
            get
            {
                return (WorkerElement)BaseGet(key);
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
            return new WorkerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WorkerElement)element).Name;
        }
    }
}
