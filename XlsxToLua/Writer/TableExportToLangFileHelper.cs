using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class TableExportToLangFileHelper
{
    public static Dictionary<string, string> LangContents = new Dictionary<string, string>();
    public const string LangKeyDelimiterString = "_";
    public const string LangFileDelimiterString = "\t";
    public const string LangFileKeyNameString = "ID";
    public const string LangFileName = "LangTable.txt";


    public static bool ExportTableToLangContent(TableInfo tableInfo, LangContent langContent, out string errorString)
    {
        errorString = null;

        // 主键列，用来生成LangKey
        List<FieldInfo> allField = tableInfo.GetAllClientFieldInfo();
        int columnCount = allField.Count;
        FieldInfo keyColumnFieldInfo = tableInfo.GetKeyColumnFieldInfo();
        int rowCount = keyColumnFieldInfo.Data.Count;

        List<string> langExportFields = new List<string>();
        if (langContent != null)
        {
            foreach (var itr in langContent.LangFields)
            {
                for (int column = 0; column < columnCount; ++column)
                {
                    FieldInfo fieldInfo = allField[column];
                    string fieldName = fieldInfo.FieldName;
                    // 多语言仅生成string/lang类型的字段
                    if (fieldName.Equals(itr.FieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (fieldInfo.DataType == DataType.String || fieldInfo.DataType == DataType.Lang)
                        {
                            langExportFields.Add(fieldName);
                        }
                        else
                        {
                            Utils.LogWarning(string.Format("表格{0}中{1}字段的类型不是字符串，请检查是否确定要导出？注意：只需导出字符串(string)类型的字段！！！", tableInfo.TableName, fieldName));
                        }
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < langExportFields.Count; ++i)
        {
            string fieldName = langExportFields[i];
            FieldInfo fieldInfo = tableInfo.GetFieldInfoByFieldName(fieldName);
            if (fieldInfo == null)
            {
                errorString = string.Format("生成{0}，但找不到{1}中的{2}项", LangFileName, tableInfo.TableName, fieldName);
                return false;
            }
            if (fieldInfo.DataType != DataType.String && fieldInfo.DataType != DataType.Lang)
            {
                Utils.LogWarning(string.Format("表格{0}中{1}字段的类型不是字符串，请检查是否确定要导出？注意：只需导出字符串(string)类型的字段！！！", tableInfo.TableName, fieldName));
                continue;
            }

            string langKeyPrefix = GetLangKeyPrefix(tableInfo, fieldName);

            for (int row = 0; row < rowCount; ++row)
            {
                string langKey = string.Format("{0}{1}", langKeyPrefix, GetLangKeyString(tableInfo, row));
                string langValue = fieldInfo.Data[row].ToString();

                // 如果文本内容为空，则不导出
                // 如果是_开头的索引值，则不导出
                if (!string.IsNullOrEmpty(langValue) && !langValue.StartsWith(langKeyPrefix))
                {
                    string lastLangValue;
                    if (LangContents.TryGetValue(langKey, out lastLangValue))
                    {
                        LangContents[langKey] = langValue;
                        Utils.LogWarning(string.Format("LangFile: 行{0}中{1}字段的文本和之前内容重复： (key={2} value={3}), 覆盖掉上一个内容last value={4}", row + 6, fieldName, langKey, langValue, lastLangValue));
                    }
                    else
                    {
                        LangContents.Add(langKey, langValue);
                    }
                    //Utils.Log(string.Format("Lang: key={0} value={1}", langKey, langValue));
                }
                else
                {
                    Utils.Log(string.Format("LangFile: 行{0}中{1}字段的文本内容为空，忽略之，不导出该键值(key={2} value={3})", row + 6, fieldName, langKey, langValue));
                }
            }
        }

        return false;
    }

    public static string GetLangKeyPrefix(TableInfo tableInfo, string fieldName)
    {
        string langKeyPrefix = string.Format("{0}{1}{2}{3}", LangKeyDelimiterString, tableInfo.TableName.ToUpper(), LangKeyDelimiterString, fieldName.ToUpper());
        return langKeyPrefix;
    }

    public static string GetLangKeyString(TableInfo tableInfo, int row)
    {
        string valueString = string.Empty;
        if (tableInfo.Keys != null && tableInfo.Keys.Count > 0)
        {
            foreach(string itr in tableInfo.Keys)
            {
                FieldInfo fieldInfo = tableInfo.GetFieldInfoByFieldName(itr);
                valueString += LangKeyDelimiterString;
                valueString += fieldInfo.Data[row].ToString().ToUpper();
            }
        }
        else
        {
            FieldInfo keyColumnFieldInfo = tableInfo.GetKeyColumnFieldInfo();
            if (keyColumnFieldInfo != null)
            {
                valueString += LangKeyDelimiterString;
                valueString += keyColumnFieldInfo.Data[row].ToString().ToUpper();
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
                writer.WriteLine(string.Format("{0}{1}{2}", "索引", LangFileDelimiterString, "中文"));
                writer.WriteLine(string.Format("{0}{1}{2}", LangFileKeyNameString, LangFileDelimiterString, "CN"));

                foreach (var itr in LangContents)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(itr.Key);
                    stringBuilder.Append(LangFileDelimiterString);
                    stringBuilder.Append(itr.Value.Replace(LangFileDelimiterString, "    "));

                    writer.WriteLine(stringBuilder);
                }

                writer.Flush();
            }
        }

        return true;
    }
    
}
