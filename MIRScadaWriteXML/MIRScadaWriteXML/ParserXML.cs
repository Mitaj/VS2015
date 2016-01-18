using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MIRScadaWriteXML
{
    class ParserXml : IDisposable
    {
        private XmlDocument _xmlDoc;
        private string _fileNameXml;
        private bool _disposed = false;
        public ParserXml(string pathToXml) {
            _fileNameXml = pathToXml;
            _xmlDoc = new XmlDocument(); // Create an XML document object
            FileStream fileStream = new FileStream(pathToXml, FileMode.Open);
            _xmlDoc.Load(fileStream); // Load the XML document from the specified file
            fileStream.Close();
            fileStream.Dispose();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {

            if (_disposed)
                return;
            if (disposing) {
                _xmlDoc.RemoveAll();
            }

            _disposed = true;

        }
       
        public bool IsSchemaFile() {
            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName("schema");

            if (paramXml.Count != 1) return false;
            foreach (XmlNode xmlN in paramXml)
            {
                if (xmlN["variables"] != null && xmlN["properties"] != null && xmlN["events"] != null && xmlN["scripts"] != null && xmlN["objects"] != null) 
                    return true;
            }

            return false;
        }

        public void ViewParams(string tagName,string paramName){

            // Get elements
            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(tagName);

            // Display the results
            for (int i = 0; i < paramXml.Count; i++)
                Console.WriteLine(paramName+": " + paramXml[i].Attributes[paramName].Value);
            Console.ReadLine();

        }

        public string GetRowInfoParam(string oTag, TbParamsXml rowData) {
            string result = "";
            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(oTag);
            bool firstElement = true;
            foreach (XmlNode xmlTag in paramXml)
            {
                if (xmlTag.Attributes["className"].Value == rowData.ClassName)
                {
                    bool validDataParam = false;
                    foreach (XmlNode xmlChild in xmlTag["properties"])
                    {
                        if (xmlChild.Attributes["name"].Value == rowData.Param)
                        {
                            validDataParam = (rowData.Attr == "" || rowData.Attr != "" && (rowData.VRegexp != "" ? ValidStringWithRegExp(rowData.VRegexp, xmlChild.Attributes[rowData.Attr].Value) : (xmlChild.Attributes[rowData.Attr] != null && xmlChild.Attributes[rowData.Attr].Value == rowData.Value)));
                            if (validDataParam)
                            {
                                if (firstElement)
                                {
                                    result += "\r\n\tClassName = \"" + xmlTag.Attributes["className"].Value + "\"  Name = \"" + xmlTag.Attributes["objectName"].Value + "\"";
                                    firstElement = false;
                                }
                                result += "\r\n\t\t" + xmlChild.OuterXml;
                            }
                        }
                    }
                    foreach (XmlNode xmlChild in xmlTag["properties"])
                    {
                        if (rowData.UseP && (xmlChild.Attributes["name"].Value == rowData.Param1 || xmlChild.Attributes["name"].Value == rowData.Param2 || xmlChild.Attributes["name"].Value == rowData.Param3 || xmlChild.Attributes["name"].Value == rowData.Param4))
                        {
                            if (validDataParam)
                                result += "\r\n\t\t" + xmlChild.OuterXml;

                        }
                    }
                    firstElement = true;
                }
            }
                return result; 
        }

        public void ReWritePropertyParamInObject(string oTag,string className, string par, string attr, string val, string vregexp) {

            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(oTag);
            bool hasEdit = false;
            foreach (XmlNode xmlTag in paramXml) {
                if (xmlTag.Attributes["className"].Value == className)
                {
                    foreach (XmlNode xmlChild in xmlTag["properties"])
                    {
                        if (xmlChild.Attributes["name"].Value == par && xmlChild.Attributes[attr]!=null && ValidStringWithRegExp(vregexp, xmlChild.Attributes[attr].Value))
                        {
                            xmlChild.Attributes[attr].Value = val;
                            hasEdit = true;
                        }
                    }
                }
                
            }
            if(hasEdit)
                _xmlDoc.Save(_fileNameXml);
        }

        public bool ValidStringWithRegExp(string regExpr, string value) {

            if (regExpr == null || regExpr == "")
                return true;

            Regex regex = new Regex(@regExpr);
            bool test = regex.IsMatch(value);
            if (regex.IsMatch(value))
                return true;

            return false;
        }

        public string GetFullValueFromParams(XmlNode xmlTag,string inputValue, string param, string attr, string regexp, string alias){

            if (inputValue == "" || param == "" || attr == "") return inputValue;
            string resultValue = null;
           
            XmlNode replaceNode = GetXmlNodeWithAttr(xmlTag, "name", param);
            Regex regex = new Regex(@"{param[1234]{1}:bool}");
            bool isBoolean = regex.IsMatch(alias);
           
            if (replaceNode != null && replaceNode.Attributes != null &&  replaceNode.Attributes[attr].Value != null)
            {
                string replaceAliasValue;
                if (replaceNode.Attributes[attr].Value != "" && ValidStringWithRegExp(regexp, replaceNode.Attributes[attr].Value))
                {
                    resultValue = inputValue;
                    replaceAliasValue = replaceNode.Attributes[attr].Value;
                    if (isBoolean)
                    {
                        replaceAliasValue = "1";
                    }
                    resultValue = resultValue.Replace(alias, replaceAliasValue);
                }
                else
                {
                    if (isBoolean)
                    {
                        replaceAliasValue = "0";
                        resultValue = inputValue;
                        resultValue = resultValue.Replace(alias, replaceAliasValue);
                    }
                    else if (regex.IsMatch(inputValue))
                    {

                        Regex param1 = new Regex(".*{param");
                        Regex param2 = new Regex(":bool}.*");
                        string replace = "";
                        string inputParamN = param2.Replace(param1.Replace(inputValue,replace),replace);
                        string aliasParamN = param1.Replace(alias, replace);
                        aliasParamN = aliasParamN.Replace("}", "");
                        if (inputParamN == aliasParamN)
                        {
                            if (resultValue == null)
                                resultValue = inputValue;
                        }
                    }
                }
            }
                
            return resultValue;
        }

        public void ReWritePropertyFromRowData(string oTag, TbParamsXml rowData)
        {

            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(oTag); 
            bool useTemplate = rowData.UseTmp;
            bool hasEdit = false;
            foreach (XmlNode xmlTag in paramXml)
            {
                if (rowData.UseTmp)
                {
                    XmlNode curNode = xmlTag;
                    bool findTag = false;
                    string[] listOfElements = rowData.Template.Split('.');
                    for (int i = 0; i < listOfElements.Length; i++)
                    {
                        
                        if (i == 0)
                        {
                            if (curNode.Attributes != null && (curNode.Attributes["objectName"] == null || listOfElements[i] != curNode.Attributes["objectName"].Value))
                                break;
                            else if (i == listOfElements.Length - 1) {

                               hasEdit = SetValueTagByParams(curNode, rowData);
                            }

                        }
                        else
                        {
                            foreach (XmlNode xmlItem in curNode.ChildNodes)
                            {
                                if (xmlItem.Attributes != null && (xmlItem.Name == oTag && xmlItem.Attributes["objectName"] != null && listOfElements[i] == xmlItem.Attributes["objectName"].Value))
                                {
                                    findTag = true;
                                    curNode = xmlItem;
                                    if (i == listOfElements.Length - 1)
                                    {
                                       hasEdit = SetValueTagByParams(xmlItem, rowData);
                                    }
                                }
                            }
                            if (!findTag)
                                break;

                        }
                    }
                }
                else hasEdit = SetValueTagByParams(xmlTag, rowData);

                if (hasEdit)
                    _xmlDoc.Save(_fileNameXml);
               
            }
            
        
        }

        public string GetReplaceXmlTagValue(XmlNode xmlTag, TbParamsXml rowData)
        {
            string replaceValue = rowData.Value.ToString();

            if (rowData.UseP && replaceValue != null && (rowData.Regexp1 != "" || replaceValue.IndexOf("{param1}")!=-1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param1, rowData.Attr1, rowData.Regexp1, "{param1}");
                if(replaceValue == rowData.Param2)
                    replaceValue = "{param2}";
                if(replaceValue == rowData.Param3)
                    replaceValue = "{param3}";
                if (replaceValue == rowData.Param4)
                    replaceValue = "{param4}";
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp1 != "" || replaceValue.IndexOf("{param1:bool}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param1, rowData.Attr1, rowData.Regexp1, "{param1:bool}");
            }
            if (rowData.UseP && replaceValue != null &&  replaceValue != "" && (rowData.Regexp2 != "" || replaceValue.IndexOf("{param2}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param2, rowData.Attr2, rowData.Regexp2, "{param2}");
                if (replaceValue == rowData.Param3)
                    replaceValue = "{param3}";
                if (replaceValue == rowData.Param4)
                    replaceValue = "{param4}";
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp2 != "" || replaceValue.IndexOf("{param2:bool}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param2, rowData.Attr2, rowData.Regexp2, "{param2:bool}");
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp3 != "" || replaceValue.IndexOf("{param3}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param3, rowData.Attr3, rowData.Regexp3, "{param3}");
                if (replaceValue == rowData.Param4)
                    replaceValue = "{param4}";
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp3 != "" || replaceValue.IndexOf("{param3:bool}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param3, rowData.Attr3, rowData.Regexp3, "{param3:bool}");
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp4 != "" || replaceValue.IndexOf("{param4}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param4, rowData.Attr4, rowData.Regexp4, "{param4}");
            }
            if (rowData.UseP && replaceValue != null && replaceValue != "" && (rowData.Regexp4 != "" || replaceValue.IndexOf("{param4:bool}") != -1))
            {
                replaceValue = GetFullValueFromParams(xmlTag["properties"], replaceValue, rowData.Param4, rowData.Attr4, rowData.Regexp4, "{param4:bool}");
            }
            return replaceValue;

        }

        public bool SetValueTagByParams(XmlNode xmlTag,TbParamsXml rowData){
            bool result = false;
            bool find_property = false;
            if (xmlTag.Attributes != null && xmlTag.Attributes["className"].Value == rowData.ClassName)
            {
                        XmlElement xmlChildren = xmlTag["properties"];
                        if (xmlChildren != null)
                            foreach (XmlNode xmlChild in xmlChildren)
                            {   
                                if (xmlChild.Attributes != null && xmlChild.Attributes["name"].Value == rowData.Param && ValidStringWithRegExp(rowData.VRegexp, xmlChild.Attributes["textValue"].Value))
                                {
                                    find_property = true;
                                    string replaceValue = GetReplaceXmlTagValue(xmlTag, rowData);
                                    if (replaceValue != null)
                                    {
                                        xmlChild.Attributes["textValue"].Value = replaceValue;
                                        result = true;
                                    } 

                                }
                            }
                        if (!find_property)
                        {
                            //if not find property Param, search Param1 and copy and rename to Param
                            if (xmlChildren != null)
                                foreach (XmlNode xmlChild in xmlChildren)
                                {   
                                    if (xmlChild.Attributes != null && xmlChild.Attributes["name"].Value == rowData.Param1)
                                    {
                                        XmlNode xmlChildClone = xmlChild.Clone();
                                        if (xmlChildClone.Attributes != null)
                                        {
                                            xmlChildClone.Attributes["name"].Value = rowData.Param;

                                            string replaceValue = GetReplaceXmlTagValue(xmlTag, rowData);
                                            if (replaceValue != null)
                                            {
                                                xmlChildClone.Attributes["textValue"].Value = replaceValue;
                                            }
                                        }

                                        XmlElement xmlElement = xmlTag["properties"];
                                        xmlElement?.AppendChild(xmlChildClone);
                                        result = true;
                                    }
                                }
                        }
                        find_property = false;
                    }
            return result;
    
        }

        public XmlNode GetXmlNodeWithAttr(XmlNode xmlTags,string namePr, string valPr) {
           
            foreach (XmlNode xmlT in xmlTags) {
                if (xmlT.Attributes != null && xmlT.Attributes[namePr].Value == valPr) return xmlT;
            }
            return null;
        }

        public bool ExistParam(string param, string tagName, string paramName)
        {

            
            // Get elements
            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(tagName);

            for (var i = 0; i < paramXml.Count; i++)
            {
                XmlAttributeCollection xmlAttributeCollection = paramXml[i].Attributes;
                if(xmlAttributeCollection != null && param == xmlAttributeCollection[paramName].Value) return true;
            }

            return false;
        }

        public string[] GetParams(string tagName, string paramName)
        {
            StringCollection strColl = new StringCollection();
            // Get elements
            XmlNodeList paramXml = _xmlDoc.GetElementsByTagName(tagName);

            // Display the results
            for (var i = 0; i < paramXml.Count; i++)
            {
                XmlAttributeCollection xmlAttributeCollection = paramXml[i].Attributes;
                if (xmlAttributeCollection != null && !IsExistInStringCollection(xmlAttributeCollection[paramName].Value, strColl))
                {
                    XmlAttributeCollection attributeCollection = paramXml[i].Attributes;
                    if (attributeCollection != null) strColl.Add(attributeCollection[paramName].Value);
                }
            }

            string[] strMResult = SortStringCollection(strColl);
       
            return strMResult;

        }

        public string[] GetInnerParams(string tagName, string className ,string innerTag, string paramName)
        {
            StringCollection strColl = new StringCollection();
            // Get elements
           // XmlNodeList paramXml = xmlDoc.GetElementsByTagName(tagName);
            string str = "//" + tagName + "[@className=\"" + className + "\"]//" + innerTag;
            XmlNodeList paramXml = _xmlDoc.SelectNodes(str);
            // Display the results
            for (var i = 0; i < paramXml.Count; i++)
            {
                XmlAttributeCollection xmlAttributeCollection = paramXml[i].Attributes;
                if (xmlAttributeCollection != null && (!IsExistInStringCollection(xmlAttributeCollection["name"].Value, strColl) && xmlAttributeCollection[paramName].Value != ""))
                {
                    XmlAttributeCollection attributeCollection = paramXml[i].Attributes;
                    if (attributeCollection != null) strColl.Add(attributeCollection["name"].Value);
                }
            }

            string[] strMResult = SortStringCollection(strColl);

            return strMResult;

        }

        private string[] SortStringCollection(StringCollection strColl) {
            string[] result = new string[strColl.Count];
            strColl.CopyTo(result, 0);
            var strSort = from s in result
                          orderby s
                          select s;
            result = strSort.ToArray();
            return result;
        
        }

        private bool IsExistInStringCollection(string strToAdd,StringCollection strColl) {
            
            if (strToAdd == "") return true;
            for (int i = 0; i < strColl.Count; i++){

                if(strColl[i] == strToAdd) return true;

            }
            return false;
        }
    }
}
