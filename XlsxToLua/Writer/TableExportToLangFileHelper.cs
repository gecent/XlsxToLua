using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class TableExportToLangFileHelper
{
    public static Dictionary<string, string> LangContents = new Dictionary<string, string>();
    public const string LangSplitString = "\t";


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

            string langKeyPrefix = string.Format("_{0}_{1}", tableInfo.TableName, fieldName);

            for (int row = 0; row < rowCount; ++row)
            {
                string langKey = string.Format("{0}_{1}", langKeyPrefix, GetKeyFieldValueString(tableInfo, row));
                string langValue = fieldInfo.Data[row].ToString();

                if (langValue.StartsWith(langKeyPrefix))
                {
                    LangContents.Add(langKey, langValue);
                    Utils.Log(string.Format("Lang: key={0} value={1}", langKey, langValue));
                }
                else
                {
                    Utils.Log(string.Format("Lang: Skipped key={0} value={1}", langKey, langValue));
                }
            }
        }

        return false;
    }

    public static string GetKeyFieldValueString(TableInfo tableInfo, int row)
    {
        FieldInfo keyColumnFieldInfo = tableInfo.GetKeyColumnFieldInfo();
        string valueString = string.Empty;
        if (keyColumnFieldInfo != null)
        {
            valueString += keyColumnFieldInfo.Data[row].ToString();
        }

        return valueString;
    }

    public static bool SaveLangFile()
    {
        if (LangContents.Count > 0)
        {
            string savePath = AppValues.ExcelFolderPath + "/_lang/cn/LangTable.txt";
            using (StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false)))
            {
                // 最上两行
                writer.WriteLine(string.Format("{0}{1}{2}", "索引", LangSplitString, "中文"));
                writer.WriteLine(string.Format("{0}{1}{2}", "id", LangSplitString, "cn"));

                foreach (var itr in LangContents)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(itr.Key);
                    stringBuilder.Append(LangSplitString);
                    stringBuilder.Append(itr.Value);

                    writer.WriteLine(stringBuilder);
                }

                writer.Flush();
            }
        }

        return true;
    }
    
}
