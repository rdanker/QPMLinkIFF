

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Data;

namespace QPMLinksoftwareNew
{
    public class QpmLinkMultipleStore
    {
        //private string Url;
        //private int Mandant;

        private QPMGetId.IdP QpmGetIdService;
        private QPMQuery.DBA QpmQueryService;
        private QPMStore.RA2 QpmStoreService;


        public string ErrorInd = "*ERROR*";
        const double NullVal = -1234567890.0123;
        const string NullValString = "-1234567890.0123";

        // 15-APR-2014. Multiple items version.

        int[] ImplicitVarPropId;

        QPMStore.PsPropValWrapper[] HeaderPropValWrapperBuffer;
        QPMStore.PoiProValWrapper[] VarPropValWrapperBuffer;
        int HeaderPropValWrapperBufferHighestIndex;
        int VarPropValWrapperBufferHighestIndex;

        public int Write = 1;
        public int Update = 2;

        public QpmLinkMultipleStore(string pUrl, int pMandant)
        {
            //Url = pUrl;
            //Mandant = pMandant;
            ImplicitVarPropId = new int[5000]; 
            HeaderPropValWrapperBuffer = new QPMStore.PsPropValWrapper[100];
            VarPropValWrapperBuffer = new QPMStore.PoiProValWrapper[300];
            HeaderPropValWrapperBufferHighestIndex = -1;
            VarPropValWrapperBufferHighestIndex = -1;
        }

        public string QpmLogin(string LoginName, string Password)
        {
            string Result = "";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(IgnoreCertificateErrorHandler);

                // IWebProxy DefaultProxy = HttpWebRequest.DefaultWebProxy; // Proxy
                // DefaultProxy.Credentials = CredentialCache.DefaultCredentials;

                QpmQueryService = new QPMQuery.DBA();
                QpmQueryService.Url = Form1.URL + "/DBA/Config1?style=document";
                QpmQueryService.Credentials = new System.Net.NetworkCredential(Form1.QPMUsername, Form1.QPMPassword);
                // QpmQueryService.Proxy = DefaultProxy;

                QpmGetIdService = new QPMGetId.IdP();
                QpmGetIdService.Url = Form1.URL + "/IdP/Config1?style=document";
                QpmGetIdService.Credentials = new System.Net.NetworkCredential(Form1.QPMUsername, Form1.QPMPassword);
                // QpmGetIdService.Proxy = DefaultProxy;

                QpmStoreService = new QPMStore.RA2();
                QpmStoreService.Url = Form1.URL + "/RA2/Config1?style=document";
                QpmStoreService.Credentials = new System.Net.NetworkCredential(Form1.QPMUsername, Form1.QPMPassword);

                // QpmStoreService.Proxy = DefaultProxy;

                // 17-SEP-2011: Login updated related to mandant security fix.
                // 20-JAN-2014: 'AND TSSetNameID = 1' added to minimise data (this process is not required to exist for this test).

                string SelectStatement =
                    "SELECT tss.TSSetNameId FROM BCC_HAI_TSSET tss WHERE tss.Mandant = " + Form1.Mandant.ToString() + " AND tss.TSSetNameId = 1";
                object[][] QueryResult = QpmQueryService.select(SelectStatement, new object[0]);
                if (QueryResult.Length > 0)
                {
                    object[] Row;
                    if (QueryResult[0] == null)
                    {
                        Row = QueryResult[1];
                        Result = "Error when logging in: " + Row[0].ToString();
                        
                    }
                }
            }
            catch (Exception f)
            {
                Result = "QPM login for user " + LoginName + " failed: " + f.ToString();

                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return Result;
        }

        private bool IgnoreCertificateErrorHandler(object Sender,
                                                   System.Security.Cryptography.X509Certificates.X509Certificate Cert,
                                                   System.Security.Cryptography.X509Certificates.X509Chain Ch,
                                                   System.Net.Security.SslPolicyErrors PolErr)
        {
            return true;
        }

        public object[][] ExecQuery(string SelectStatement, object[] DateTimePar)
        {
            object[][] QueryResult = null;
            try
            {
                object[] Row;
                try
                {
                    QueryResult = QpmQueryService.select(SelectStatement, DateTimePar);
                    if (QueryResult.Length > 0)
                    {
                        if (QueryResult[0] == null)
                        {
                            Row = QueryResult[1];
                            Row[1] = Row[0].ToString();
                            Row[0] = ErrorInd;
                        }
                    }
                }
                catch (Exception ex)
                {
                    BudaLibrary.GeneralFunctions.err_handle(ex.Message.Trim(), ex.StackTrace.Trim());
                    Row = new object[2];
                    Row[0] = ErrorInd;
                    Row[1] = ex.ToString();
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return QueryResult;
        }
        public object[][] ExecQuery2(string SelectStatement, DateTime EventDateTime)
        {
            //start and end date 
            string startdate = EventDateTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime startDt = DateTime.ParseExact(startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string enddate = EventDateTime.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDt = DateTime.ParseExact(enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            object[] DateTimePar = new object[] { startDt,endDt};

            object[][] QueryResult = null;
            object[] Row;
            try
            {
                QueryResult = QpmQueryService.select(SelectStatement, DateTimePar);
                if (QueryResult.Length > 0)
                {
                    if (QueryResult[0] == null)
                    {
                        Row = QueryResult[1];
                        Row[1] = Row[0].ToString();
                        Row[0] = ErrorInd;
                    }
                }
                else
                {
                    QueryResult = null;
                }
            }
            catch (Exception ex)
            {
                BudaLibrary.GeneralFunctions.err_handle(ex.Message.Trim(), ex.StackTrace.Trim());
                Row = new object[2];
                Row[0] = ErrorInd;
                Row[1] = ex.ToString();
            }
            return QueryResult;
        }
        public DataTable ExecQueryToTable(string SelectStatement)
        {
            DataTable ret = new DataTable();
            DataColumn dc0 = new DataColumn("date", Type.GetType("System.DateTime"));
            ret.Columns.Add(dc0);

            try
            {
                object[] EventDT = new object[0];

                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        for (int i = 0; i <= QueryResult.Length - 1; i++)
                        {
                            DataRow drow = ret.NewRow();
                            drow["date"] = QueryResult[i][0].ToString().Trim();
                            ret.Rows.Add(drow);
                        }
                    }
                }
            }
            catch(Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }
        public string StoreData(int ProcessID, DateTime EventDT, string PropOrVar, int PropOrVarID, int SampleNumber, string Value, int Mode)
        {
            string ReturnValue = "";
            try
            {
                string EventDateTime = EventDT.ToString("M/d/yyyy H:mm:ss.fff");
                if (BudaLibrary.BudaUtil.myCult.Name.Trim() == "de_DE")
                {
                    EventDateTime = EventDateTime.Replace('.', '/') + ".000";
                }
                else
                {
                    EventDateTime = EventDateTime.Replace('-', '/') + ".000";
                }
                int?[] SampleNumberArray = new int?[1];
                SampleNumberArray[0] = SampleNumber;
                string[] SampleValueArray = new string[1];
                SampleValueArray[0] = Value;
                string ServiceCallResult;
                if (PropOrVar == "P")
                {
                    QPMStore.PsPropValWrapper[] HeaderPropValWrapper = new QPMStore.PsPropValWrapper[1];
                    QPMStore.PsPropValWrapper SingleHeaderPropValWrapper = new QPMStore.PsPropValWrapper();
                    SingleHeaderPropValWrapper.mandant = Convert.ToInt32(Form1.Mandant);
                    SingleHeaderPropValWrapper.tsSetNameId = ProcessID;
                    SingleHeaderPropValWrapper.psDateTime = EventDateTime;
                    SingleHeaderPropValWrapper.propNameId = PropOrVarID;
                    SingleHeaderPropValWrapper.sampleNum = SampleNumberArray;
                    SingleHeaderPropValWrapper.propVal = SampleValueArray;
                    HeaderPropValWrapper.SetValue(SingleHeaderPropValWrapper, 0);
                    if (Mode == this.Write)
                    {
                        ServiceCallResult = QpmStoreService.addPsPropVal(HeaderPropValWrapper);
                    }
                    else // Update
                    {
                        ServiceCallResult = QpmStoreService.modifyPsPropVal(HeaderPropValWrapper);
                    }
                }
                else
                {
                    int ImplicitID = GetImplicitPropId(Convert.ToInt32(Form1.Mandant), ProcessID, PropOrVarID);
                    QPMStore.PoiProValWrapper[] VarPropValWrapper = new QPMStore.PoiProValWrapper[1];
                    QPMStore.PoiProValWrapper SingleVarPropValWrapper = new QPMStore.PoiProValWrapper();
                    SingleVarPropValWrapper.mandant = Convert.ToInt32(Form1.Mandant);
                    SingleVarPropValWrapper.tsSetNameId = ProcessID;
                    SingleVarPropValWrapper.psDateTime = EventDateTime;
                    SingleVarPropValWrapper.tsNameId = PropOrVarID;
                    SingleVarPropValWrapper.propNameId = ImplicitID;
                    SingleVarPropValWrapper.sampleNum = SampleNumberArray;
                    SingleVarPropValWrapper.propVal = SampleValueArray;
                    VarPropValWrapper.SetValue(SingleVarPropValWrapper, 0);
                    if (Mode == this.Write)
                    {
                    ServiceCallResult = QpmStoreService.addPoiProVal(VarPropValWrapper);
                    }
                    else // Update
                    {
                        ServiceCallResult = QpmStoreService.modifyPoiProVal(VarPropValWrapper);
                    }
                }
                if (ServiceCallResult.Substring(0, 1) != "0") // 11-05-2012. Updated because first character is not always a digit. 
                {
                    ReturnValue = ServiceCallResult;
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ReturnValue;
        }

        private int GetImplicitPropId(int MandantId, int ProcessId, int VarId)
        {
            int PropId = 0;
            if (ImplicitVarPropId[VarId] > 0)
            {
                PropId = ImplicitVarPropId[VarId];
            }
            else
            {
                PropId = QpmGetIdService.getImplicitId(MandantId, ProcessId, VarId);
                ImplicitVarPropId[VarId] = PropId;
            }
            return PropId;
        }

// ----------------------------------------------------------------------------------------- Multiple data store/update 

        public void MultipleDataBufferInit() // 15-APR-2014. New multiple data routines.
        {
            while (HeaderPropValWrapperBufferHighestIndex >= 0)
            {
                HeaderPropValWrapperBuffer[HeaderPropValWrapperBufferHighestIndex] = null;
                HeaderPropValWrapperBufferHighestIndex--;
            }
            while (VarPropValWrapperBufferHighestIndex >= 0)
            {
                VarPropValWrapperBuffer[VarPropValWrapperBufferHighestIndex] = null;
                VarPropValWrapperBufferHighestIndex--;
            }
            // Both HeaderPropValWrapperHighestIndex and VarPropValWrapperHighestIndex are now equal to -1.
        }

        public string MultipleDataLoad(int ProcessID, DateTime EventDT, string PropOrVar, int PropOrVarID, int SampleNumber, string Value)
        {
            string ReturnValue = "";
            try
            {
                string EventDateTime = EventDT.ToString("M/d/yyyy H:mm:ss.fff");
                //if (BudaLibrary.BudaUtil.myCult.Name.Trim() == "de_DE")
                //{
                //    EventDateTime = EventDateTime.Replace('.', '/') + ".000";
                //}
                //else
                //{
                    EventDateTime = EventDateTime.Replace('-', '/') + ".000";
                //}
                int?[] SampleNumberArray = new int?[1];
                SampleNumberArray[0] = SampleNumber;
                string[] SampleValueArray = new string[1];
                SampleValueArray[0] = Value;
                if (PropOrVar == "P")
                {
                    QPMStore.PsPropValWrapper SingleHeaderPropValWrapper = new QPMStore.PsPropValWrapper();
                    SingleHeaderPropValWrapper.mandant = Convert.ToInt32(Form1.Mandant); ;
                    SingleHeaderPropValWrapper.tsSetNameId = ProcessID;
                    SingleHeaderPropValWrapper.psDateTime = EventDateTime;
                    SingleHeaderPropValWrapper.propNameId = PropOrVarID;
                    SingleHeaderPropValWrapper.sampleNum = SampleNumberArray;
                    SingleHeaderPropValWrapper.propVal = SampleValueArray;
                    HeaderPropValWrapperBuffer.SetValue(SingleHeaderPropValWrapper, ++HeaderPropValWrapperBufferHighestIndex);
                }
                else
                {
                    //int ImplicitID = GetImplicitPropId(Mandant, ProcessID, PropOrVarID);
                    int ImplicitID = PropOrVarID + 1;
                    QPMStore.PoiProValWrapper SingleVarPropValWrapper = new QPMStore.PoiProValWrapper();
                    SingleVarPropValWrapper.mandant = Convert.ToInt32(Form1.Mandant); ;
                    SingleVarPropValWrapper.tsSetNameId = ProcessID;
                    SingleVarPropValWrapper.psDateTime = EventDateTime;
                    SingleVarPropValWrapper.tsNameId = PropOrVarID;
                    SingleVarPropValWrapper.propNameId = ImplicitID;
                    SingleVarPropValWrapper.sampleNum = SampleNumberArray;
                    SingleVarPropValWrapper.propVal = SampleValueArray;
                    VarPropValWrapperBuffer.SetValue(SingleVarPropValWrapper, ++VarPropValWrapperBufferHighestIndex);
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ReturnValue;
        }
        public string MultipleDataStore(int Mode)
        {
            string ReturnValue = "";
            try
            {
                string ServiceCallResult = ""; 

                if (HeaderPropValWrapperBufferHighestIndex >= 0)
                {
                    QPMStore.PsPropValWrapper[] HeaderPropValWrapper = new QPMStore.PsPropValWrapper[HeaderPropValWrapperBufferHighestIndex + 1];
                    for (int i = 0; i < HeaderPropValWrapper.Length; i++)
                    {
                        HeaderPropValWrapper[i] = HeaderPropValWrapperBuffer[i];
                    }
                    if (Mode == this.Write)
                    {
                        ServiceCallResult = QpmStoreService.addPsPropVal(HeaderPropValWrapper);
                    }
                    else // Update
                    {
                        try
                        {
                            if (QpmStoreService.modifyPsPropVal(HeaderPropValWrapper) != null)
                            {
                                ServiceCallResult = QpmStoreService.modifyPsPropVal(HeaderPropValWrapper);
                            }
                        }
                        catch (Exception f)
                        {
                            BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
                        }
                    }
                    if (ServiceCallResult.Substring(0, 1) != "0") // 11-05-2012. Updated because first character is not always a digit. 
                    {
                        ReturnValue += "\n" + ServiceCallResult;
                    }
                }
                if (VarPropValWrapperBufferHighestIndex >= 0)
                {
                    QPMStore.PoiProValWrapper[] VarPropValWrapper = new QPMStore.PoiProValWrapper[VarPropValWrapperBufferHighestIndex + 1];
                    for (int i = 0; i < VarPropValWrapper.Length; i++)
                    {
                        VarPropValWrapper[i] = VarPropValWrapperBuffer[i];
                    }
                    if (Mode == this.Write)
                    {
                        ServiceCallResult = QpmStoreService.addPoiProVal(VarPropValWrapper);
                    }
                    else // Update
                    {
                        ServiceCallResult = QpmStoreService.modifyPoiProVal(VarPropValWrapper);
                    }
                    if (ServiceCallResult.Substring(0, 1) != "0") // 11-05-2012. Updated because first character is not always a digit. 
                    {
                        ReturnValue += "\n" + ServiceCallResult;
                    }
                }
                MultipleDataBufferInit(); // Clean up to make sure and to make it easier for debugging. 
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ReturnValue;
        }
        public DateTime GetLatestPropValue(int ProcessId, int PropId)
        {
            
            string LatestPropValue = "";
            DateTime LatestPropDT = DateTime.MinValue;
            try
            {
                DateTime EventDateTime = Convert.ToDateTime("1-9-2018");

                string EDT = EventDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                EventDateTime = DateTime.ParseExact(EDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                object[] EventDT = new object[] { EventDateTime };

                string SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM BCC_HAI_PSPropVal pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ? ORDER BY EventDT DESC";
                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        string DT = Row[0].ToString();
                        LatestPropDT = DateTime.ParseExact(DT, "yyyy-MM-dd HH:mm:ss.FFF", CultureInfo.InvariantCulture);
                        LatestPropValue = Row[1].ToString(); // PropValue
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(),f.StackTrace.Trim());
            }
            return LatestPropDT;
        }
        public void deleteEvent(string QPMProcessId,DateTime dtEvent)
        {
            try
            {

                DateTime dateFrom = dtEvent.AddSeconds(-1);
                DateTime dateTo = dtEvent.AddSeconds(1);

                string datefrom = dateFrom.ToString("M/d/yyyy H:mm:ss");
                if (BudaLibrary.BudaUtil.myCult.Name.Trim() == "de_DE")
                {
                    datefrom = datefrom.Replace('.', '/') + ".000";
                }
                else
                {
                    datefrom = datefrom.Replace('-', '/') + ".000";
                }

                string dateto = dateTo.ToString("M/d/yyyy H:mm:ss");
                if (BudaLibrary.BudaUtil.myCult.Name.Trim() == "de_DE")
                {
                    dateto = dateto.Replace('.', '/') + ".000";
                }
                else
                {
                    dateto = dateto.Replace('-', '/') + ".000";
                }



                QpmStoreService.delete(Convert.ToInt32(Form1.Mandant), Convert.ToInt32(QPMProcessId), datefrom, dateto);
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }
        public void storeSpec(int QPMProcessId, int depPropId, string depPropValue, int propNameId, int tsNameId, DateTime start1, double lil, double lsl, double lcl, double target, double ucl, double usl, double uil, int sampleSize, int status)
        {
            try
            {
                string start = start1.ToString("M/d/yyyy H:mm:ss.fff");
                start = start.Replace('-', '/');

                QPMSpec.Spec qpmspec = new QPMSpec.Spec();
                qpmspec.add(Convert.ToInt32(Form1.Mandant), QPMProcessId, depPropId, depPropValue, propNameId, tsNameId, start, lil, lsl, lcl, target, ucl, usl, uil, sampleSize, status);
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
        }
        public DataTable GetPropValueNew(int ProcessId, int PropId, DateTime EventDateTime)
        {
            DataTable ret = new DataTable();


            try
            {
                DataColumn dc0 = new DataColumn("value", Type.GetType("System.String"));
                DataColumn dc1 = new DataColumn("date", Type.GetType("System.DateTime"));

                ret.Columns.Add(dc0);
                ret.Columns.Add(dc1);

                string EDT = EventDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                EventDateTime = DateTime.ParseExact(EDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                object[] EventDT = new object[] { EventDateTime };

                string SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM BCC_HAI_PSPropVal pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ? ORDER BY EventDT";
                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        for (int i = 0; i <= QueryResult.Length - 1; i++)
                        {
                            DataRow drow = ret.NewRow();

                            drow["date"] = QueryResult[i][0].ToString().Trim();
                            if (QueryResult[i][1] != null)
                            {
                                drow["value"] = QueryResult[i][1].ToString().Trim();
                            }
                            ret.Rows.Add(drow);
                        }
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }
        public DateTime? getDateTime(string ProcessId,string PropId, string propValue)
        {
            DateTime? ret = null;
            try
            {
                object[] EventDT = new object[0];

                //string SelectStatement = "SELECT pspv.PSDATETIME AS EventDT FROM BCC_HAI_POIPROVAL pspv WHERE pspv.MANDANT = " + Form1.Mandant.ToString() + " AND  pspv.TSSETNAMEID = " + ProcessId.ToString() + " AND pspv.PROPNAMEID = " + PropId.ToString() + " AND pspv.PROPVAL = '" + propValue.Trim() + "' ORDER BY pspv.PSDATETIME  DESC";
                string SelectStatement = "SELECT PSDATETIME AS EventDT FROM " + Form1.SAPPrefix.Trim() + ".BCC_HAI_POIPROVAL WHERE MANDANT = " + Form1.Mandant.ToString() + " AND  TSSETNAMEID = " + ProcessId.ToString() + " AND PROPNAMEID = " + PropId.ToString().Trim() + " AND PROPVAL like '" + propValue.Trim() + "%' ORDER BY EventDT DESC";
                object[][] QueryResult = QpmQueryService.select(SelectStatement,EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        if (QueryResult[0][0].ToString().Trim() != "")
                        {
                            ret = Convert.ToDateTime(QueryResult[0][0].ToString().Trim());
                        }
                    }
                }
            }
            catch(Exception f)
            {

            }
            return ret;
        }
        public object[][] getValue(string ProcessId,string PropId, bool PropOrVar,DateTime EventDateTime)
        {
            object[][] ret = null;
            try
            {
                DateTime startDate = EventDateTime.AddSeconds(-1);
                DateTime endDate = EventDateTime.AddSeconds(1);
                string SDT = startDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Trim();
                startDate = DateTime.ParseExact(SDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string EDT = endDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Trim();
                endDate = DateTime.ParseExact(EDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                object[] EventDT = new object[] { startDate,endDate };

                string SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM BCC_HAI_POIPROVAL pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ? AND pspv.PSDateTime < ? ";
                if (PropOrVar == true)
                {
                    SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM BCC_HAI_PSPropVal pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ?  AND pspv.PSDateTime < ?";
                }
               
                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        ret = QueryResult;
                    }
                }
            }
            catch(Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }

        public DataTable GetPropValueLatest2(int ProcessId, int PropId, DateTime EventDateTime)
        {
            DataTable ret = new DataTable();


            try
            {
                DataColumn dc0 = new DataColumn("value", Type.GetType("System.String"));
                DataColumn dc1 = new DataColumn("date", Type.GetType("System.DateTime"));

                ret.Columns.Add(dc0);
                ret.Columns.Add(dc1);

                string EDT = EventDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                EventDateTime = DateTime.ParseExact(EDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                object[] EventDT = new object[] { EventDateTime };

                string SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM BCC_HAI_PSPropVal pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ? ORDER BY EventDT DESC";
                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        for (int i = 0; i <= QueryResult.Length - 1; i++)
                        {
                            DataRow drow = ret.NewRow();

                            drow["date"] = QueryResult[i][0].ToString().Trim();
                            if (QueryResult[i][1] != null)
                            {
                                drow["value"] = QueryResult[i][1].ToString().Trim();
                            }
                            ret.Rows.Add(drow);
                        }
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }
        public DataTable GetValueNew(int ProcessId, int PropId, bool PropOrVar,DateTime EventDateTime)
        {
            DataTable ret = new DataTable();


            try
            {
                DataColumn dc0 = new DataColumn("value", Type.GetType("System.String"));
                DataColumn dc1 = new DataColumn("date", Type.GetType("System.DateTime"));

                ret.Columns.Add(dc0);
                ret.Columns.Add(dc1);

                DateTime startDate = EventDateTime.AddSeconds(-1);
                DateTime endDate = EventDateTime.AddSeconds(1);
                string SDT = startDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Trim();
                startDate = DateTime.ParseExact(SDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string EDT = endDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Trim();
                endDate = DateTime.ParseExact(EDT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                object[] EventDT = new object[] { startDate, endDate };

                string SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM " + Form1.SAPPrefix + ".BCC_HAI_POIPROVAL pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ? AND pspv.PSDateTime < ? ";
                if (PropOrVar == true)
                {
                    SelectStatement = "SELECT pspv.PSDateTime AS EventDT, pspv.PropVal FROM " + Form1.SAPPrefix + ".BCC_HAI_PSPropVal pspv WHERE pspv.Mandant = " + Form1.Mandant.ToString() + " AND  pspv.TSSetnameID = " + ProcessId.ToString() + " AND pspv.PropNameID = " + PropId.ToString() + " AND pspv.PSDateTime > ?  AND pspv.PSDateTime < ?";
                }
                object[][] QueryResult = QpmQueryService.select(SelectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        for (int i = 0; i <= QueryResult.Length - 1; i++)
                        {
                            DataRow drow = ret.NewRow();

                            drow["date"] = QueryResult[i][0].ToString().Trim();
                            if (QueryResult[i][1] != null)
                            {
                                drow["value"] = QueryResult[i][1].ToString().Trim();
                            }
                            ret.Rows.Add(drow);
                        }
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }

        public DataTable GetProcessAndDate(string value,string varId)
        {
            DataTable ret = new DataTable();
            DataColumn dc0 = new DataColumn("process", Type.GetType("System.String"));
            DataColumn dc1 = new DataColumn("date", Type.GetType("System.DateTime"));
          
            ret.Columns.Add(dc0);
            ret.Columns.Add(dc1);

            try
            {
                object[] EventDT = new object[0];

                string selectStatement = "SELECT ppv.PSDATETIME,ppv.TSSETNAMEID,ppv.PROPVAL FROM " + Form1.SAPPrefix.Trim() + ".BCC_HAI_POIPROVAL ppv WHERE ppv.Mandant = " + Form1.Mandant.Trim() + " AND ppv.PROPVAL = '" + value.Trim() + "' AND ppv.PROPNAMEID = " + varId.Trim();
                object[][] QueryResult = QpmQueryService.select(selectStatement, EventDT);
                if (QueryResult.Length > 0)
                {
                    object[] Row = QueryResult[0];
                    if (Row == null)
                    {
                        Row = QueryResult[1];
                    }
                    else
                    {
                        for (int i = 0; i <= QueryResult.Length - 1; i++)
                        {
                            DataRow drow = ret.NewRow();

                            drow["date"] = QueryResult[i][0].ToString().Trim();
                            if (QueryResult[i][1] != null)
                            {
                                drow["process"] = QueryResult[i][1].ToString().Trim();
                            }
                            ret.Rows.Add(drow);
                        }
                    }
                }
            }
            catch (Exception f)
            {
                BudaLibrary.GeneralFunctions.err_handle(f.Message.Trim(), f.StackTrace.Trim());
            }
            return ret;
        }
    }
}
