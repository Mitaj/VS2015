using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MIRScadaWriteXML
{   
    class ProjectParserXml
    {   private static List<string> _allValidXmlFiles;
        private static string[][] _assocFiles;
      
        public ProjectParserXml(string pathToXmlf) {
            string[] allXmlFiles = Directory.GetFiles(pathToXmlf, "*.xml", SearchOption.AllDirectories);
            _allValidXmlFiles = new List<string>();
            ParserXml parItm;

            foreach (string strF in allXmlFiles)
            {
                parItm = new ParserXml(strF);
                if (parItm.IsSchemaFile())
                    _allValidXmlFiles.Add(strF);

                parItm.Dispose();
            }

            _assocFiles = new string[_allValidXmlFiles.Count][];
            
        }

        public string GetInfoParamsFromFiles(IEnumerable<TbParamsXml> allParams)
        {

            /*fileName
                ClassName
                    property
                    property1
                    property2
                    property3
                    property4
            */
            string result = "";
            IEnumerable<TbParamsXml> sortParams = allParams.OrderBy(param => param.ClassName);
            foreach (string strF in _allValidXmlFiles)
            {
                ParserXml parseXml = new ParserXml(strF);
                string fileParams = strF;
                string className = "";
                foreach (TbParamsXml rowData in sortParams)
                {  
                    if (rowData.ClassName != className)
                    {
                        className = rowData.ClassName;
                       // fileParams += "\r\n\t" + className;
                    }

                    string paramInfo = parseXml.GetRowInfoParam("object", rowData);
                    if (paramInfo != null && paramInfo != "")
                    {
                        fileParams += paramInfo;
                    }
                   

                }
                parseXml.Dispose();
                fileParams += "\r\n";
                result += fileParams;
            }
            return result;
        }

        public void EditObjectParamsInXml(string classN, string par, string attr, string val, string vregexp) {
            foreach (string strF in _allValidXmlFiles) {
                ParserXml parseXml = new ParserXml(strF);
                parseXml.ReWritePropertyParamInObject("object", classN,par, attr, val, vregexp);
                parseXml.Dispose();
            }
        }

        public void EditObjectParamsInXmlEx(TbParamsXml rowData/*string classN, string par, string par2, string template*/)
        {
            foreach (string strF in _allValidXmlFiles)
            {
                ParserXml parseXml = new ParserXml(strF);
                parseXml.ReWritePropertyFromRowData("object", rowData);
                parseXml.Dispose();
            }
        }
        public string[] GetAllUsedParams() {

            string[] result;
            StringCollection strColl = new StringCollection();
            ParserXml parseXml;
            Console.WriteLine("Всего файлов на сканировании: " + _allValidXmlFiles.Count);
            for (int i = 0; i < _allValidXmlFiles.Count; i++) {
                
                parseXml = new ParserXml(_allValidXmlFiles[i]);
                string[] strToAdd =  parseXml.GetParams("object", "className");
                _assocFiles[i] = new string[strToAdd.Length];
                
                for (int j = 0; j < strToAdd.Length;j++)
                {
                    _assocFiles[i][j] = strToAdd[j];
                    if (!strColl.Contains(strToAdd[j])) strColl.Add(strToAdd[j]);

                }
                parseXml.Dispose();

            }

            result = SortStringCollection(strColl);

            return result;

        }
        
        public string[] GetFilesWhereParam(string className){
            string[] result;
            StringCollection strFilesColl = new StringCollection();
            for (int i = 0; i < _assocFiles.Length; i++) {

                for (int j = 0; j < _assocFiles[i].Length; j++) {

                    if (_assocFiles[i][j] == className) {

                        strFilesColl.Add(_allValidXmlFiles[i]);
                        break;
                    }
                
                }
                
            
            }

            if (strFilesColl.Count > 0)
            {

                result = new string[strFilesColl.Count];
                strFilesColl.CopyTo(result, 0);

            }
            else result = null;
               
            return result;
        }

        public string[] GetPropertiesObjectInFile(string className, string filePath)
        {
            string[] result;
            ParserXml parseXml;
            parseXml = new ParserXml(filePath);
            result = parseXml.GetInnerParams("object", className, "property", "textValue");
            parseXml.Dispose();
            return result;
        }

        private string[] SortStringCollection(StringCollection strColl)
        {
            string[] result = new string[strColl.Count];
            strColl.CopyTo(result, 0);
            var strSort = from s in result
                          orderby s
                          select s;
            result = strSort.ToArray();
            return result;

        }
    }
}
