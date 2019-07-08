using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class TableExportToLangFileHelper
{
    public static Dictionary<string, string> LangContents = new Dictionary<string, string>();
    public const string LangKeySplitString = "_";
    public const string LangFileSplitString = "\t";
    public const string LangFileName = "LangTable.txt";


    public static bool ExportTableToLangContent(TableInfo tableInfo, List<LangField> langFields, out string errorString)
    {
        errorString = null;

        // 主键列，用来生成LangKey
        FieldInfo keyColumnFieldInfo = tableInfo.GetKeyColumnFieldInfo();
        int rowCount = keyColumnFieldInfo.Data.Count;

        for (int i = 0; i < langFields.Count; ++i)
        {
            string fieldName = langFields[i].FieldName;
            FieldInfo fieldInfo = tableInfo.GetFieldInfoByFieldName(fieldName);
            if (fieldInfo == null)
            {
                errorString = string.Format("生成LangFile，但找不到{0}中的{1}项", tableInfo.TableName, fieldName);
                return false;
            }

            string langKeyPrefix = string.Format("{0}{1}{2}{3}", LangKeySplitString, tableInfo.TableName, LangKeySplitString, fieldName);

            for (int row = 0; row < rowCount; ++row)
            {
                string langKey = string.Format("{0}{1}", langKeyPrefix, GetKeyFieldValueString(tableInfo, row));
                string langValue = fieldInfo.Data[row].ToString();

                if (!langValue.StartsWith(langKeyPrefix))
                {
                    string lastLangValue;
                    if (LangContents.TryGetValue(langKey, out lastLangValue))
                    {
                        LangContents[langKey] = langValue;
                        Utils.LogWarning(string.Format("LangFile: 重复： key={0} new value={1},    覆盖掉上一个内容value={2}", langKey, langValue, lastLangValue));
                    }
                    else
                    {
                        LangContents.Add(langKey, langValue);
                    }
                    //Utils.Log(string.Format("Lang: key={0} value={1}", langKey, langValue));
                }
                else
                {
                    Utils.Log(string.Format("LangFile: 忽略：不导出该键值 key={0} value={1}", langKey, langValue));
                }
            }
        }

        return false;
    }

    public static string GetKeyFieldValueString(TableInfo tableInfo, int row)
    {
        string valueString = string.Empty;
        if (tableInfo.Keys != null && tableInfo.Keys.Count > 0)
        {
            foreach(string itr in tableInfo.Keys)
            {
                FieldInfo fieldInfo = tableInfo.GetFieldInfoByFieldName(itr);
                valueString += LangKeySplitString;
                valueString += fieldInfo.Data[row].ToString();
            }
        }
        else
        {
            FieldInfo keyColumnFieldInfo = tableInfo.GetKeyColumnFieldInfo();
            if (keyColumnFieldInfo != null)
            {
                valueString += LangKeySplitString;
                valueString += keyColumnFieldInfo.Data[row].ToString();
            }
        }

        return valueString;
    }

    public static bool SaveLangFile()
    {
        if (LangContents.Count > 0)
        {
            string savePath = AppValues.ExcelFolderPath + "/_lang/cn/" + LangFileName;
            Utils.Log(string.Format("开始保存文件{0}", savePath));
            using (StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false)))
            {
                // 最上两行
                writer.WriteLine(string.Format("{0}{1}{2}", "索引", LangFileSplitString, "中文"));
                writer.WriteLine(string.Format("{0}{1}{2}", "id", LangFileSplitString, "cn"));

                foreach (var itr in LangContents)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(itr.Key);
                    stringBuilder.Append(LangFileSplitString);
                    stringBuilder.Append(itr.Value);

                    writer.WriteLine(stringBuilder);
                }

                writer.Flush();
            }
        }

        return true;
    }
    
}
