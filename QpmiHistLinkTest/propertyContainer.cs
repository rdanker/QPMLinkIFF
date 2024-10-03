using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace QPMLinksoftwareNew
{
    public class propertyContainer : ObservableCollection<property>
    {
        public string processName { get; set; }
        public string grouping_key { get; set; }
    }
    public class property
    {
        public string propertyName { get; set; }
        public string stringValue { get; set; }
        public double doubleValue { get; set; }
        public int intValue { get; set; }
        public DateTime dateValue { get; set; }
        public bool boolValue { get; set; }
        public string datatype { get; set; }

    }
}
