using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.OleDb;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Common;
using System.Windows;
using System.IO;
using System.Xml;
using System.Net.Http;
using Newtonsoft.Json;
using ProXcel.Service.Interface.ClientLibrary;
using ProXcel.Service.Interface.Contracts;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Drawing.Printing;


namespace QPMLinksoftwareNew
{

    public partial class Form1 : Form
    {
        public bool Cancel;

        private const int AliasWidth = 25;
        private const int TagNameWidth = 40;
        public static string QPMUsername, QPMPassword, Mandant, QPMLanguageId, SAPPrefix, URL, constring2, constring, SAPUser, SAPPassword, SAPMandant, company, typeTagValue;
        public static string SAPName, SAPAppServerHost, SAPClient, SAPSystemNumber, SAPLanguage, SAPPoolsize, SAPPeakConnectionsLimit,proXcel;
        public static string QlipPath, NutricontrolPath;
        public static int IdleSampleTime, StartupPhaseSampleTime, BatchPhaseSampleTime;
        public DateTime actualStartDate, plannedStartDate, plannedEndDate, dt;
        public ArrayList interfaceList = new ArrayList();
        private QpmLinkMultipleStore QW;
        private Thread thread;
        public double addDay = 0;
        public DataTable dtTable;
        public DateTime lastDate;
        bool bConnectionSucceeded = false;

        public string constringhist = "Provider=iHOLEDB.iHistorian.1;Persist Security Info=False;Data Source=NLTBHISTAPPV1;Mode=Read;";
        private OleDbConnection OleDbConn = null;


        public Form1()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception f)
            {
                MessageBox.Show(f.Message.Trim());
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {


                Cancel = false;
                BudaLibrary.BudaUtil.myCulture = new CultureInfo("en-us");
                BudaLibrary.BudaUtil.myCult = System.Globalization.CultureInfo.CurrentCulture;
                BudaLibrary.BudaUtil.dbProvider = System.Configuration.ConfigurationManager.AppSettings.Get("dbProvider");
                BudaLibrary.BudaUtil.ConnectionString = ConfigurationManager.AppSettings.Get("connectionString");
                BudaLibrary.BudaUtil.ConnectionString2 = ConfigurationManager.AppSettings.Get("connectionString2");
                QPMUsername = ConfigurationManager.AppSettings.Get("QPMUsername");
                QPMPassword = ConfigurationManager.AppSettings.Get("QPMPassword");
                URL = ConfigurationManager.AppSettings.Get("URL");
                QPMLanguageId = ConfigurationManager.AppSettings.Get("QPMLanguageId");
                SAPPrefix = ConfigurationManager.AppSettings.Get("SAPPrefix");
                Mandant = ConfigurationManager.AppSettings.Get("Mandant");
                constring2 = ConfigurationManager.AppSettings.Get("connectionString2");
                company = ConfigurationManager.AppSettings.Get("company");

                int defaultLanguageId = 1;

                if (Int32.TryParse(ConfigurationManager.AppSettings.Get("defaultLanguageId"), out defaultLanguageId))
                {
                    BudaLibrary.BudaUtil.DefaultLanguageId = defaultLanguageId;
                }

                int defaultCompanyId = 1;

                if (Int32.TryParse(ConfigurationManager.AppSettings.Get("defaultCompanyId"), out defaultCompanyId))
                {
                    BudaLibrary.BudaUtil.DefaultCompanyId = defaultCompanyId;
                }

                BudaLibrary.BudaUtil.CompanyId = 1;
                BudaLibrary.BudaUtil.LanguageId = 1;
                initialiseInterfaces();

                string selInterfaceTypes = "SELECT DISTINCT driver FROM interfaces_oudedata WHERE active = 1";
                DataTable dtInterfaceTypes =  BudaLibrary.DatabaseAdapter.SelectSQL(selInterfaceTypes.Trim());
                cbInterfaceTypes.DataSource = dtInterfaceTypes;
                cbInterfaceTypes.ValueMember = "driver";
                cbInterfaceTypes.DisplayMember = "driver";
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message.Trim());
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }

        public void initialiseInterfaces()
        {
            try
            {
                //Ophalen van de actieve interfaces
                //Let op voorlopig even 1 login

                string selInterfaces = "SELECT * FROM interfaces WHERE active = 1";
                DataTable dtInterfaces = BudaLibrary.DatabaseAdapter.SelectSQL(selInterfaces.Trim());
                for (int i = 0; i <= dtInterfaces.Rows.Count - 1; i++)
                {
                    string interfaceId = dtInterfaces.Rows[i]["id"].ToString().Trim();
                    string interfaceName = dtInterfaces.Rows[i]["interfaceDesc"].ToString().Trim();
                    string constring = dtInterfaces.Rows[i]["constring"].ToString().Trim();
                    string query = dtInterfaces.Rows[i]["query"].ToString().Trim();
                    int resourceId = 0;
                    if (dtInterfaces.Rows[i]["ResId"].ToString().Trim() != "")
                    {

                        resourceId = Convert.ToInt32(dtInterfaces.Rows[i]["ResId"].ToString().Trim());
                    }
                    string driver = dtInterfaces.Rows[i]["driver"].ToString().Trim();
                    string eventName = dtInterfaces.Rows[i]["eventName"].ToString().Trim();
                    string eventDate = dtInterfaces.Rows[i]["eventDateTag"].ToString().Trim();
                    string fileName = dtInterfaces.Rows[i]["fileName"].ToString().Trim();
                    string tableName = dtInterfaces.Rows[i]["tableName"].ToString().Trim();
                    string statusTag = dtInterfaces.Rows[i]["statusTag"].ToString().Trim();
                    string stopTag = dtInterfaces.Rows[i]["stopTag"].ToString().Trim();
                    string QPMProcessId = dtInterfaces.Rows[i]["QPMProcessId"].ToString().Trim();
                    double interval = 120000;
                    if (dtInterfaces.Rows[i]["interval"].ToString().Trim() != "")
                    {
                        interval = Convert.ToDouble(dtInterfaces.Rows[i]["interval"].ToString().Trim());
                    }
                    string objectType = dtInterfaces.Rows[i]["objectType"].ToString().Trim();
                    string templateId = dtInterfaces.Rows[i]["templateId"].ToString().Trim();
                    string subtype = dtInterfaces.Rows[i]["subtypeId"].ToString().Trim();
                    string userName = dtInterfaces.Rows[i]["logonName"].ToString().Trim();
                    string password = dtInterfaces.Rows[i]["logonPassword"].ToString().Trim();
                     DateTime lastScanDate = DateTime.Now;
                    if (dtInterfaces.Rows[i]["scanDate"].ToString().Trim() != "")
                    {
                        lastScanDate = Convert.ToDateTime(dtInterfaces.Rows[i]["scanDate"].ToString().Trim());
                    }
                    string eventId = dtInterfaces.Rows[i]["eventId"].ToString().Trim();

                    SQLinterface sqlinterface = new SQLinterface();
                    sqlinterface.interfaceId = interfaceId;
                    sqlinterface.interfaceName = interfaceName;
                    sqlinterface.constring = constring;
                    sqlinterface.resourceId = resourceId;
                    sqlinterface.eventName = eventName.Trim();
                    sqlinterface.eventDate = eventDate.Trim();
                    sqlinterface.statusTag = statusTag;
                    sqlinterface.stopTag = stopTag;
                    sqlinterface.query = query;
                    sqlinterface.tableName = tableName;
                    sqlinterface.fileName = fileName;
                    sqlinterface.interval = interval;
                    sqlinterface.objectType = objectType;
                    sqlinterface.templateId = templateId;
                    sqlinterface.driver = driver;
                    sqlinterface.QPMProcessId = QPMProcessId;
                    sqlinterface.lastScanDate = lastScanDate;
                    sqlinterface.userName = userName.Trim();
                    sqlinterface.password = password.Trim();
                    sqlinterface.eventId = eventId.Trim();
                    interfaceList.Add(sqlinterface);
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime DateTimeStart = DateTime.Now;
                bool bConnectionSucceeded = false;

                //start.IsEnabled = false;
                //stop.IsEnabled = true;
                //test.IsEnabled = false;

                for (int i = 0; i <= interfaceList.Count - 1; i++)
                {
                    SQLinterface _sqlInterface = (SQLinterface)interfaceList[i];
                    thread = new Thread(() => DoServiceWork(_sqlInterface));
                    thread.Start();
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }
        private void stop_Click()
        {
            Cancel = true;
        }

        private void DoServiceWork(SQLinterface _sqlInterface)
        {
            try
            {
                int startInterval = Convert.ToInt32(_sqlInterface.interval);

                //You have the interface id and the frequency
                while (!Cancel)
                {
                    DateTime DateTimeStart = DateTime.Now; // Start of processing
                    
                    //Depending on the  type of interface choose the right function

                    switch (_sqlInterface.driver)
                    {
                        case "iHistorian":
                            //DateTimeStart = _sqlInterface.lastScanDate;
                            DateTimeStart = DateTime.Now;
                            functIHistorian(_sqlInterface, DateTimeStart);
                            break;
                    }

                    DateTime DatetTimeEnd = DateTime.Now;
                    TimeSpan Duration = DatetTimeEnd - DateTimeStart; // Duration of processing
                    int Interval = startInterval - (int)Duration.TotalMilliseconds;
                    while (Interval <= 0) // if processing takes longer than the interval....
                    {
                        Interval = Interval + (int)_sqlInterface.interval;
                    }
                    Thread.Sleep(Interval);
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }
        private void test_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startdate = System.DateTime.Now;
                SQLinterface _sqlinterface = (SQLinterface)interfaceList[0];



                if (cbInterfaceTypes.SelectedValue != null)
                {
                    string driver = cbInterfaceTypes.SelectedValue.ToString().Trim();

                    string selInterfaces = "SELECT * FROM interfaces WHERE active = 1 AND driver ='" + driver + "'";
                    DataTable dtInterfaces = BudaLibrary.DatabaseAdapter.SelectSQL(selInterfaces.Trim());

                    ArrayList _interfaceList = new ArrayList();
                    for (int i=0;i<=interfaceList.Count-1;i++)
                    {
                        if (((SQLinterface)interfaceList[i]).driver.Trim() == driver.Trim())
                        {
                            _interfaceList.Add(interfaceList[i]);
                        }
                    }
                    switch (driver)
                    {
                        case "iHistorian":
                            for (int i = 0; i <= _interfaceList.Count - 1; i++)
                            {
                                SQLinterface _sqlInterface = (SQLinterface)_interfaceList[i];
                                //startdate = _sqlinterface.lastScanDate;
                                startdate = DateTime.Now;
                                functIHistorian(_sqlInterface, startdate);
                            }
                            break;                       
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }

        private void functIHistorian(SQLinterface _sqlInterface, DateTime startdate)
        {
            try
            {
                //The handling of the iHistorian interface

                DateTime eventDate = startdate;
                EventData eventdata = new EventData();

                string QPMProcessId = _sqlInterface.QPMProcessId.ToString().Trim();

                //Get the related tags

                string selTags = "SELECT * FROM interfaceTags WHERE active = 1 AND interfaceId  = " + _sqlInterface.interfaceId.ToString().Trim();
                DataTable dtTags = BudaLibrary.DatabaseAdapter.SelectSQL(selTags.Trim());

                int shiftNumber = 1;

                for (int i = 0; i <= dtTags.Rows.Count - 1; i++)
                {
                    string tagName = dtTags.Rows[i]["tagName"].ToString().Trim();
                    string tagAlias = dtTags.Rows[i]["tagAlias"].ToString().Trim();
                    string tagDesc = dtTags.Rows[i]["tagDesc"].ToString().Trim();

                    string delimStr = "_";
                    char[] delimiter = delimStr.ToCharArray();

                    string[] split = null;
                    split = tagName.Split(delimiter);

                    string propOrVar = split[0];
                    string variable = split[1];

                    bool prop = false;
                    if (propOrVar.Trim() == "P")
                    {
                        prop = true;
                    }

                    string eventdate = eventDate.ToString("yyyy-MM-dd HH:mm:ss").Trim();
                    DataTable dtData = new DataTable();
                    string ValueDT = "";

                    if (tagDesc.Trim() != "dienst" & tagDesc.Trim() != "ploeg")
                    {
                        string selData = "SET SamplingMode=RawByNumber, StartTime='" + eventdate + "', " +
                                                            "NumberOfSamples=1, Direction=Backward " +
                        " SELECT Value FROM ihRawData WHERE TagName='" + tagAlias.Trim() + "'";
                        //dtData = BudaLibrary.DatabaseAdapter.SelectSQL("System.Data.OleDb", _sqlInterface.constring, selData.Trim());

                       

                        try
                        {
                            if (!bConnectionSucceeded)
                            {
                                OleDbConnection OleDbConn = new OleDbConnection(constringhist);
                                OleDbConn.Open(); // Open ONCE, not every scan. 12-DEC-2012.
                                bConnectionSucceeded = true;
                            }
                        }
                        catch (Exception f)
                        {
                            bConnectionSucceeded = false;
                        }

                        OleDbCommand OleDbCmd = new OleDbCommand(selData, OleDbConn);
                        OleDbDataReader OleDbReader = OleDbCmd.ExecuteReader();
                        if (OleDbReader.Read())
                        {
                            ValueDT = OleDbReader.GetString(0).ToString();
                        }
                        OleDbReader.Close();

                    }
                    if (dtData != null)
                    {
                        if (dtData.Rows.Count > 0 || tagDesc.Trim() == "dienst" || tagDesc.Trim() == "ploeg")
                        {
                            EventDataElement dataelement = new EventDataElement();
                            dataelement.variabele = variable.Trim();
                            dataelement.property = prop;

                            string datatype = dtTags.Rows[i]["type"].ToString().Trim();
                            string waarde = "";
                            if (dtData.Rows.Count > 0)
                            {
                                waarde = dtData.Rows[0]["Value"].ToString().Trim();
                            }

                            switch (datatype)
                            {
                            case "string":

                                dataelement.eventDataType = "string";

                                #region shift

                                string selShift = "SELECT * FROM shifts WITH(NOLOCK) WHERE active = 1 AND resourceId = " + QPMProcessId.Trim();
                                DataTable dtShifts = BudaLibrary.DatabaseAdapter.SelectSQL(selShift.Trim());
                                {
                                    string shiftVariableId = dtShifts.Rows[0]["shiftVariableId"].ToString().Trim();
                                    if (shiftVariableId.Trim() == variable.Trim())
                                    {
                                        //The shift variable
                                        //Take the simple way. 
                                        //Only the hours and minutes

                                        double currentime = eventDate.Hour * 60 + eventDate.Minute;

                                        for (int z = 0; z <= dtShifts.Rows.Count - 1; z++)
                                        {
                                            int starttime = Convert.ToInt32(dtShifts.Rows[z]["starttime"].ToString().Trim());
                                            int endtime = Convert.ToInt32(dtShifts.Rows[z]["endtime"].ToString().Trim());

                                            if (starttime < endtime)
                                            {
                                                if (currentime >= starttime & currentime < endtime)
                                                {
                                                    dataelement.stringValue = dtShifts.Rows[z]["shiftId"].ToString().Trim();
                                                    shiftNumber = Convert.ToInt32(dtShifts.Rows[z]["shiftNumber"].ToString().Trim());
                                                    waarde = shiftNumber.ToString().Trim();
                                                }
                                            }
                                            else
                                            {
                                                //Night shift

                                                if (currentime < endtime || currentime > starttime)
                                                {
                                                    dataelement.stringValue = dtShifts.Rows[z]["shiftId"].ToString().Trim();
                                                    shiftNumber = Convert.ToInt32(dtShifts.Rows[z]["shiftNumber"].ToString().Trim());
                                                    waarde = shiftNumber.ToString().Trim();
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                //Restriction the shift variable has to be in the interfaceTags tale first, otherwise you have to determine the shifs twice.

                                #region ploeg

                                string selTeams = "SELECT * FROM teams WITH(NOLOCK) WHERE active = 1 AND resourceId = '" + QPMProcessId.Trim() + "'";
                                DataTable dtTeams = BudaLibrary.DatabaseAdapter.SelectSQL(selTeams.Trim());

                                string selTeamSequence = "SELECT * FROM teamSequence WITH(NOLOCK) WHERE active = 1 AND resourceId = '" + QPMProcessId.Trim() + "' ORDER BY seq";
                                DataTable dtTeamSequence = BudaLibrary.DatabaseAdapter.SelectSQL(selTeamSequence.Trim());

                                string teamVariableId = dtTeams.Rows[0]["teamVariableId"].ToString().Trim();

                                if (variable.Trim() == teamVariableId.Trim())
                                {
                                    #region determine the monday of week 1

                                    string selDateFirstJanuary = "1-1-" + System.DateTime.Now.Year.ToString().Trim();
                                    DateTime dtFirstJanuary = Convert.ToDateTime(selDateFirstJanuary.Trim());

                                    DayOfWeek dayOfWeek = dtFirstJanuary.DayOfWeek;

                                    int daysBack = 0;

                                    switch (dayOfWeek)
                                    {
                                        case DayOfWeek.Monday:
                                            daysBack = 7;
                                            break;
                                        case DayOfWeek.Tuesday:
                                            daysBack = 6;
                                            break;
                                        case DayOfWeek.Wednesday:
                                            daysBack = 5;
                                            break;
                                        case DayOfWeek.Thursday:
                                            daysBack = -3;
                                            break;
                                        case DayOfWeek.Friday:
                                            daysBack = -4;
                                            break;
                                        case DayOfWeek.Saturday:
                                            daysBack = -5;
                                            break;
                                        case DayOfWeek.Sunday:
                                            daysBack = -6;
                                            break;
                                    }

                                    DateTime firstMonday = dtFirstJanuary.AddDays(daysBack);

                                    #endregion

                                    #region determine the weeknumber 

                                    double weeknumber = Math.Floor(eventDate.Date.Subtract(firstMonday).TotalDays / 7) + 1;

                                    #endregion

                                    #region determine the productioncycle period

                                    int productionCycle = dtTeamSequence.Rows.Count;

                                    #endregion

                                    #region determine the starting team

                                    double cyclenumber = weeknumber / productionCycle;
                                    int cyclenumber2 = (int)Math.Floor(cyclenumber);

                                    int cyclenumber3 = (int)Math.Ceiling(cyclenumber - cyclenumber2) + 1;

                                    if (cyclenumber3 == 1)
                                    {
                                        cyclenumber3 = dtShifts.Rows.Count - 1;
                                    }
                                    else
                                    {
                                        cyclenumber3 = cyclenumber3 - dtShifts.Rows.Count + 1;
                                    }

                                    string startingTeamnumber = dtTeamSequence.Rows[cyclenumber3]["teamId"].ToString().Trim();

                                    //Equal number of shifts as team

                                    string selectedTeamNumber = "";

                                    if (shiftNumber == 1)
                                    {
                                        selectedTeamNumber = startingTeamnumber.Trim();
                                    }

                                    int startRow = 0;
                                    for (int v = 0; v <= dtTeamSequence.Rows.Count - 1; v++)
                                    {
                                        //Start team

                                        if (dtTeamSequence.Rows[v]["teamId"].ToString().Trim() == startingTeamnumber.Trim())
                                        {
                                            startRow = v;
                                        }
                                    }

                                    int teller = startRow;
                                    for (int v = 0; v <= dtTeamSequence.Rows.Count - 1; v++)
                                    {
                                        if (teller < 0)
                                        {
                                            teller = dtTeamSequence.Rows.Count - 1;
                                        }
                                        if (v == shiftNumber - 1)
                                        {
                                            selectedTeamNumber = dtTeamSequence.Rows[teller]["teamId"].ToString().Trim();
                                        }
                                        teller = teller - 1;
                                    }
                                    waarde = selectedTeamNumber.Trim();
                                    #endregion
                                }

                                #endregion

                                if (waarde.Trim() != "")
                                {
                                    dataelement.stringValue = waarde.Trim();
                                }
                                else
                                {
                                    waarde = "NA";
                                    dataelement.stringValue = waarde.Trim();
                                }
                                break;
                            case "double":
                                if (waarde.Trim() != "")
                                {
                                    dataelement.floatValue = Convert.ToDouble(waarde.Trim());
                                }
                                dataelement.eventDataType = "double";
                                break;
                            case "datetime":
                                if (waarde.Trim() != "")
                                {
                                    dataelement.dateValue = Convert.ToDateTime(waarde.Trim());
                                }
                                dataelement.eventDataType = "datetime";
                                break;
                            }

                            eventdata.Add(dataelement);
                        }
                    }
                }

                QPMEvent(true, QPMProcessId.Trim(), (DateTime)eventDate, eventdata);

                eventdata.Clear();               
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }
         public void QPMEvent(bool UpdateMode, string QPMProcessId, DateTime EventDT, EventData eventdata)
        {
            try
            {
                QW = new QpmLinkMultipleStore(Form1.URL, Convert.ToInt32(Form1.Mandant));
                string test = QW.QpmLogin(Form1.QPMUsername, Form1.QPMPassword);
                QW.MultipleDataBufferInit();

                int mode = 2;

                for (int i = 0; i <= eventdata.Count - 1; i++)
                {
                    EventDataElement eventdataelement = (EventDataElement)eventdata[i];
                    string variabele = eventdataelement.variabele.Trim();
                    string Value = "";
                    switch (eventdataelement.eventDataType.Trim())
                    {
                        case "datetime":
                            if (eventdataelement.dateValue != null)
                            {
                                DateTime waarde1 = Convert.ToDateTime(eventdataelement.dateValue);
                                Value = waarde1.ToString("", CultureInfo.InvariantCulture);
                            }
                            break;
                        case "string":
                            Value = "NA";
                            if (eventdataelement.stringValue != null)
                            {
                                Value = eventdataelement.stringValue.Trim();
                            }

                            break;
                        case "double":
                            if (eventdataelement.floatValue != null)
                            {
                                double waarde2 = eventdataelement.floatValue;
                                Value = waarde2.ToString("", CultureInfo.InvariantCulture);
                            }
                            break;
                        case "bool":
                            //Nog te maken
                            break;
                    }

                    string QPMFieldId = eventdataelement.variabele.Trim();

                    string propOrVal = "V";

                    if (eventdataelement.property == true)
                    {
                        propOrVal = "P";
                    }
                    int QPMFieldId1 = Convert.ToInt32(QPMFieldId);
                    if (Value != null & Value.Trim() != "")
                    {
                        string testStore = QW.MultipleDataLoad(Convert.ToInt32(QPMProcessId), EventDT, propOrVal.Trim(), Convert.ToInt32(QPMFieldId), 1, Value);
                        if (testStore.Trim() != "")
                        {
                            BudaLibrary.GeneralFunctions.err_handle(testStore, System.DateTime.Now.ToString().Trim());
                        }
                    }
                }
                string Message = QW.MultipleDataStore(mode);

                if (Message.Trim() != "")
                {
                    BudaLibrary.GeneralFunctions.err_handle(Message.ToString().Trim(), System.DateTime.Now.ToString().Trim());
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.ToString().Trim(), f.StackTrace.Trim());
            }
        }
    }
}