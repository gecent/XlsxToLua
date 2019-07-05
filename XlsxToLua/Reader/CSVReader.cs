using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BuildData
{
    public class CSVReader
    {
        public CSVReader()
        {
        }

        public CSVReader(string fileName) : this()
        {
            ReadFile(fileName);
        }

        public void ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            using (StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(936)))
            {
                String line = sr.ReadLine(); //第一行注释不处理
                line = sr.ReadLine();       //第二行名字
                if (line != null)
                {
                    listName = ParseLine(line);
                }

                listLines = new List<List<string>>();
                while ((line = sr.ReadLine()) != null)
                {
                    List<String> list = ParseLine(line);
                    foreach (String s in list)  // 排除空行
                    {
                        if (s != "")
                        {
                            listLines.Add(ParseLine(line));
                            break;
                        }
                    }
                }
            }

        }

        public List<string> GetLine(Int32 index)
        {
            return listLines[index];
        }

        public string GetString(Int32 index, string name)
        {
            List<string> list = GetLine(index);
            if (list != null)
            {
                for (int i = 0; i < listName.Count; ++i)
                {
                    if (listName[i] == name)
                    {
                        return list[i];
                    }
                }
            }
            return "null";
        }

        private static List<String> ParseLine(String line)
        {
            bool InDoubleQuotationMark = false;
            StringBuilder builder = new StringBuilder();
            List<String> list = new List<String>();
            for (int i = 0; i < line.Length; ++i )
            {
                if (!InDoubleQuotationMark)
                {
                    if (line[i] == '"')
                    {
                        InDoubleQuotationMark = true;
                    }
                    else if (line[i] == ',')
                    {
                        list.Add(builder.ToString());
                        builder.Remove(0, builder.Length);
                    }
                    else
                    {
                        builder.Append(line[i]);
                    }

                    if (i+1 == line.Length)
                    {
                        list.Add(builder.ToString());
                    }
                }
                else
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 == line.Length || line[i + 1] != '"')
                        {
                            InDoubleQuotationMark = false;
                            list.Add(builder.ToString());
                            builder.Remove(0, builder.Length);
                        }
                        else
                        {
                            builder.Append(line[i]);
                        }
                        ++i;
                    }
                    else
                    {
                        builder.Append(line[i]);
                    }
                }
            }         

            return list;
        }

        public bool GetValueAsString(Int32 index, string name, out String value)
        {
            value = GetString(index, name);
            return value != "null";
        }

        public bool GetValueAsInt32(Int32 index, string name, out Int32 value)
        {
            String str = GetString(index, name);
            value = 0;
            if (str == "null")
            {
                return false;
            }
            else
            {
                bool bConvert = true;
                try
                {
                    value = Convert.ToInt32(str);
                }
                catch (System.Exception ex)
                {
                    bConvert = false;
                }
                return bConvert;
            }
        }

        public bool GetValueAsLong(Int32 index, string name, out long value)
        {
            String str = GetString(index, name);
            value = 0;
            if (str == "null")
            {
                return false;
            }
            else
            {
                bool bConvert = true;
                try
                {
                    value = Convert.ToInt64(str);
                }
                catch (System.Exception ex)
                {
                    bConvert = false;
                }
                return bConvert;
            }
        }

        public bool GetValueAsFloat(Int32 index, string name, out float value)
        {
            String str = GetString(index, name);
            value = 0;
            if (str == "null")
            {
                return false;
            }
            else
            {
                bool bConvert = true;
                try
                {
                    value = Convert.ToSingle(str);
                }
                catch (System.Exception ex)
                {
                    bConvert = false;
                }
                return bConvert;
            }
        }

        public bool GetValueAsListInt32(Int32 index, string name, List<Int32> list)
        {
            list.Clear();

            String str = GetString(index, name);
            if (str.Length > 0 && str != "null")
            {
                string[] strings = str.Split(new Char[] { ';' });
                foreach (string s in strings)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        list.Add(Convert.ToInt32(s));
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetValueAsListFloat(Int32 index, string name, List<float> list)
        {
            list.Clear();

            String str = GetString(index, name);
            if (str.Length > 0 && str != "null")
            {
                string[] strings = str.Split(new Char[] { ';' });
                foreach (string s in strings)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        list.Add(Convert.ToSingle(s));
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetValueAsListString(Int32 index, string name, List<String> list)
        {
            list.Clear();

            String str = GetString(index, name);
            if (str.Length > 0 && str != "null")
            {
                foreach (String s in str.Split(new Char[] {';'}))
                {
                    list.Add(s);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetValueAsDictionInt32Int32(Int32 index, String name, Dictionary<Int32, Int32> diction)
        {
            diction.Clear();
            String s = GetString(index, name);
            if (s.Length <= 0 || s == "null")
            {
                return false;
            }

            if (s[0] != '<' || s[s.Length-1] != '>')
            {
                return false;
            }

            String tmp = s.Substring(1, s.Length - 2);
            foreach (String sitem in tmp.Split(new String[] { "><" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] keyValue = sitem.Split(new Char[] { ',' });
                Int32 key = Convert.ToInt32(keyValue[0]);
                Int32 value = Convert.ToInt32(keyValue[1]);

                diction.Add(key, value);
            }

            return true;
        }

        public bool GetValueAsDictionStringString(Int32 index, String name, Dictionary<String, String> diction)
        {
            diction.Clear();
            String s = GetString(index, name);
            if (s.Length <= 0 || s == "null")
            {
                return false;
            }

            if (s[0] != '<' || s[s.Length - 1] != '>')
            {
                return false;
            }

            String tmp = s.Substring(1, s.Length - 2);
            foreach (String sitem in tmp.Split(new String[] { "><" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] keyValue = sitem.Split(new Char[] { ',' });
                diction.Add(keyValue[0], keyValue[1]);
            }

            return true;
        }

        List<string> listName;
        List<List<string>> listLines;

        public Int32 Count { get { return listLines.Count; } }

        private string fileName;
        public string FileName 
        { 
            get
            {
                return fileName;
            }
            set
            {
                fileName = FileName;
                ReadFile(fileName);
            }
        }

    }
}
