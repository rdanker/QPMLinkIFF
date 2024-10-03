using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;

namespace QPMLinksoftwareNew
{
    public class Settings
    {
        XPathDocument XmlSettings;
        public XPathNavigator XPathNav;
        public string PathPrefix;

        public Settings()
        {
            try
            {
                // The xml file is in the same directory as the program exe file.
                string ApplicationDirectory = Assembly.GetExecutingAssembly().GetName().CodeBase;
                int LastSlashPos = ApplicationDirectory.LastIndexOfAny("/".ToCharArray());
                ApplicationDirectory = ApplicationDirectory.Substring(0, LastSlashPos);
                string FullXmlPath = ApplicationDirectory + @"/QpmiHistLinkTest.xml";
                XmlSettings = new XPathDocument(FullXmlPath.Substring(8));
                XPathNav = XmlSettings.CreateNavigator();
                PathPrefix = "//Settings";
                string TestSet = GetSingleSetting("/@Test", "");
                if (TestSet != "") PathPrefix += "/" + TestSet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings" + Environment.NewLine + ex.ToString());
            }
        }

        public string GetSingleSetting(string XPathExpr, string DefaultValue)
        {
            string Result = null;
            try
            {
                Result = XPathNav.SelectSingleNode(PathPrefix + XPathExpr).Value;
            }
            catch (Exception ex)
            {
                if (DefaultValue == "")
                {
                    MessageBox.Show("Settings file problem for XPath " + XPathExpr + Environment.NewLine + ex.ToString());
                }
                Result = DefaultValue;
            }
            return Result;
        }

        public string[,] GetMultipleSettings(string XPathExpr, string[] AttributeName)
        {
            string[,] Result = null;
            XPathNodeIterator ResultNodes = null;
            try
            {
                ResultNodes = XPathNav.Select(PathPrefix + XPathExpr);
                XPathNavigator FirstNode = ResultNodes.Current;
                Result = new string[ResultNodes.Count, AttributeName.Length];
                int r = -1;
                foreach (XPathNavigator Node in ResultNodes)
                {
                    r++;
                    for (int c = 0; c < AttributeName.Length; c++)
                    {
                        Result[r, c] = Node.GetAttribute(AttributeName[c], string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings file problem for XPath " + XPathExpr + Environment.NewLine + ex.ToString());
            }
            return Result;
        }

    }
}