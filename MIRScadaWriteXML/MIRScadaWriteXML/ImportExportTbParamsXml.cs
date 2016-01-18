using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIRScadaWriteXML
{
    internal class ImportExportTbParamsXml
    {
        private List<TbParamsXml> _mainCollection = new List<TbParamsXml>();

        public List<TbParamsXml> GetParamsCollection()
        {
            return _mainCollection;
        }

        public void SetParamsCollection(List<TbParamsXml> setCollection)
        {
            _mainCollection = setCollection;
        }

        public bool SaveParamsCollection(List<TbParamsXml> tbToSaveParams, string fileName)
        {
            int length = tbToSaveParams.Count;
            if (string.IsNullOrEmpty(fileName) || length == 0) return false;

            string[] lines = new string[length];
            for (int index = 0; index < tbToSaveParams.Count; index++)
            {
                TbParamsXml itemParams = tbToSaveParams[index];
                lines[index] = itemParams.GetAllParamsString();
            }

            System.IO.File.WriteAllLines(fileName, lines, Encoding.Default);

            return true;
        }

        public bool LoadParamsCollection(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            string[] lines = System.IO.File.ReadAllLines(fileName, Encoding.Default);

            String resParams = "";
            foreach (string itemParams  in lines)
            {
                resParams = itemParams.Trim();
                if (resParams == "")
                    continue;

                TbParamsXml allParams = new TbParamsXml();
                string[] strSeparator = new string[] { "{;}" };
                string[] arrayParams = resParams.Split(strSeparator, StringSplitOptions.RemoveEmptyEntries);
                String resPar = "";
                foreach (string iParam in arrayParams)
                {
                    resPar = iParam.Trim();
                    if (resPar == "")
                        continue;
                    string[] itemValue = resPar.Split('=');
                    if (itemValue.Length != 2)
                        continue;
                    char[] charsTrim = {'"'};
                    string nameParam = itemValue[0].Trim();
                    string valueParam = itemValue[1].Trim().Trim(charsTrim);
                    if (allParams[nameParam]!=null)
                    {
                        allParams[nameParam] = valueParam;
                    }
                    

                }
                _mainCollection.Add(allParams);

            }
            return true;
        }
    }
}
