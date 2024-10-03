using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace QPMLinksoftwareNew
{
    public class EventData : ObservableCollection<EventDataElement>
    {
    }
    public class EventDataElement
    {
        public string variabele { get; set; }
        public string variableName { get; set; }
        public string eventDataType { get; set;}
        public string stringValue { get; set; }
        public double floatValue { get; set; }
        public DateTime dateValue { get; set; }
        public bool boolValue { get; set; }
        public bool property { get; set; }
    }
}
