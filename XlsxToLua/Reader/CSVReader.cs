using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class CSVReader
{
    //public static char FileDelimiterString = ',';
    public static char FileDelimiterString = '\t';

    private List<string> m_ListName;
    public List<string> ListName { get { return m_ListName; } }

    private List<List<string>> m_ListLines;

    public int Count { get { return m_ListLines.Count; } }

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
        using (StreamReader sr = new StreamReader(fileName, new UTF8Encoding(false)))
        {
            String line = sr.ReadLine(); //第一行注释不处理
            line = sr.ReadLine();       //第二行名字
            if (line != null)
            {
                m_ListName = ParseLine(line);
            }

            m_ListLines = new List<List<string>>();
            while ((line = sr.ReadLine()) != null)
            {
                List<String> list = ParseLine(line);
                foreach (String s in list)  // 排除空行
                {
                    if (s != "")
                    {
                        m_ListLines.Add(ParseLine(line));
                        break;
                    }
                }
            }
        }

    }

    public List<string> GetLine(int index)
    {
        return m_ListLines[index];
    }

    public string GetString(int index, string name)
    {
        List<string> list = GetLine(index);
        if (list != null)
        {
            for (int i = 0; i < m_ListName.Count; ++i)
            {
                if (m_ListName[i] == name)
                {
                    return list[i];
                }
            }
        }
        return "null";
    }

    public string GetStringByColumn(int index, int column)
    {
        var line = GetLine(index);
        string content = "null";
        if (column >= 0 && column < line.Count)
        {
            content = line[column];
        }
        return content;
    }

    private static List<String> ParseLine(String line)
    {
        bool InDoubleQuotationMark = false;
        StringBuilder builder = new StringBuilder();
        List<String> list = new List<String>();
        for (int i = 0; i < line.Length; ++i)
        {
            if (!InDoubleQuotationMark)
            {
                if (line[i] == '"')
                {
                    InDoubleQuotationMark = true;
                }
                else if (line[i] == FileDelimiterString)
                {
                    list.Add(builder.ToString());
                    builder.Remove(0, builder.Length);
                }
                else
                {
                    builder.Append(line[i]);
                }

                if (i + 1 == line.Length)
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

    public bool GetValueAsString(int index, string name, out String value)
    {
        value = GetString(index, name);
        return value != "null";
    }

    public bool GetValueAsInt32(int index, string name, out int value)
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

    public bool GetValueAsLong(int index, string name, out long value)
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

    public bool GetValueAsFloat(int index, string name, out float value)
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

    public bool GetValueAsListint(int index, string name, List<int> list)
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

    public bool GetValueAsListFloat(int index, string name, List<float> list)
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

    public bool GetValueAsListString(int index, string name, List<String> list)
    {
        list.Clear();

        String str = GetString(index, name);
        if (str.Length > 0 && str != "null")
        {
            foreach (String s in str.Split(new Char[] { ';' }))
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

    public bool GetValueAsDictionInt32Int32(int index, String name, Dictionary<int, int> diction)
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
            int key = Convert.ToInt32(keyValue[0]);
            int value = Convert.ToInt32(keyValue[1]);

            diction.Add(key, value);
        }

        return true;
    }

    public bool GetValueAsDictionStringString(int index, String name, Dictionary<String, String> diction)
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


}
