using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MIRScadaWriteXML
{
    public class TbParamsXml
    {
        private string[] _nameOfElements =
       {
            "Use",
            "ClassName",
            "Param",
            "Attr",
            "Value",
            "VRegexp",
            "UseP",
            "Regexp1",
            "Param1",
            "Attr1",
            "Regexp2",
            "Param2",
            "Attr2",
            "Regexp3",
            "Param3",
            "Attr3",
            "Regexp4",
            "Param4",
            "Attr4",
            "UseTmp",
            "Template"
        };
        public TbParamsXml()
        {
            foreach (string itemElement in _nameOfElements)
            {
                if (itemElement == "Use" || itemElement == "UseP" || itemElement == "UseTmp")
                    this[itemElement] = false;
                else
                    this[itemElement] = "";
            }
        }

        public bool Use { set; get; }
        public string ClassName { set; get; }
        public string Param { set; get; }
        public string Attr { set; get; }
        public string Value { set; get; }
        public string VRegexp { set; get; }
        public bool UseP { set; get; }
        public string Regexp1 { set; get; }
        public string Param1 { set; get; }
        public string Attr1 { set; get; }
        public string Regexp2 { set; get; }
        public string Param2 { set; get; }
        public string Attr2 { set; get; }
        public string Regexp3 { set; get; }
        public string Param3 { set; get; }
        public string Attr3 { set; get; }
        public string Regexp4 { set; get; }
        public string Param4 { set; get; }
        public string Attr4 { set; get; }
        public bool UseTmp { set; get; }
        public string Template { set; get; }

        public string GetAllParamsString()
        {
            string result = "";
            for (int index = 0; index < _nameOfElements.Length; index++)
            {
                string itemElement = _nameOfElements[index];
                result += itemElement + " = " + this[itemElement];
                if (index != _nameOfElements.Length - 1)
                    result += " {;} ";
            }

            return result;
        }

        public object this[string propertyName]
        {
            get
            {
                PropertyInfo piMain = this.GetType().GetProperty(propertyName);
                if (piMain == null)
                    return null;

                return piMain.GetValue(this);
            }
            set
            {
                if (propertyName == "Use" || propertyName == "UseP" || propertyName == "UseTmp")
                    value = bool.Parse(value.ToString());
                    
                this.GetType().GetProperty(propertyName).SetValue(this, value, null);
            }
        }
    }
}
