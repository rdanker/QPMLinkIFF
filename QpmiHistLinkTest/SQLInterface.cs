using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QPMLinksoftwareNew
{

    public class SQLinterface
    {
        public string interfaceId { get; set; }
        public string interfaceName { get; set; }
        public string QPMProcessId { get; set; }
        public string driver { get; set; }
        public string constring { get; set; }
        public string query { get; set; }
        public double interval { get; set; }
        public string objectType { get; set; }
        public int resourceId { get; set; }
        public string eventName { get; set; }
        public string eventDate { get; set; }
        public string tableName { get; set; }
        public string fileName { get; set; }
        public string fileNameOut { get; set; }
        public string statusTag { get; set; }
        public string statusTag1 { get; set; }
        public string stopTag { get; set; }
        public string timeTag { get; set; }
        public string idTag { get; set; }
        public string templateId { get; set; }
        public string subtype { get; set; }
        public DateTime lastScanDate { get; set; }
        public string lastNode { get; set; }
        public string typeNode { get; set; }
        public string valueNode { get; set; }
        public string outputPath { get; set; }
        public string company { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public double lookBackPeriod { get; set; }
        public string eventId { get; set; }
        public string klantCode { get; set; }
        public string referentieKlant { get; set; }
        public double scanHour { get; set; }
        public int startRow { get; set; }
        public int endRow { get; set; }

    
    }
}
