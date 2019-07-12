using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class LangField
{
    public string FieldName; // 多语言字段名
    public string FieldAbbr; // 多语言字段缩写名，用于减短关键字
}
public class LangContent
{
    public string FileName; // 表格名
    public string FileAbbr; // 表格缩写名，用于减短关键字
    public List<LangField> LangFields = new List<LangField>(); //表格中包含的所有多语言字段
}

public class LangDescriptionReader
{
    private List<string> m_FilePaths = new List<string>();
    private Dictionary<String, LangContent> m_Description = new Dictionary<string, LangContent>();

    /// <summary>
    /// key: 文件名（已经去掉路径和后缀）; value: 文件中的字段
    /// </summary>
    public Dictionary<String, LangContent> Description { get { return m_Description; } private set { Description = value; } }

    public bool Contains(string fileName)
    {
        if (Description.ContainsKey(fileName))
        {
            return true;
        }

        return false;
    }
    public bool ReadFile(String descFilePath, out string errorString)
    {
        m_FilePaths.Clear();
        m_Description.Clear();

        if (!File.Exists(descFilePath))
        {
            errorString = string.Format("错误：没有找到文件{0}", descFilePath);
            return false;
        }

        using (FileStream fileStream = new FileStream(descFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (XmlReader reader = XmlReader.Create(descFilePath))
            {
                while (reader.Read())
                {
                    if (reader.Name == "LangElement" && reader.NodeType == XmlNodeType.Element)
                    {
                        String filePath = reader.GetAttribute("File");
                        String fileAbbr = reader.GetAttribute("Abbr");
                        if (m_FilePaths.Contains(filePath))
                        {
                            m_FilePaths.Clear();
                            m_Description.Clear();
                            errorString = string.Format("错误：描述文件中的文件路径重复 {0}", filePath);
                            return false;
                        }

                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (m_Description.ContainsKey(fileName))
                        {
                            m_FilePaths.Clear();
                            m_Description.Clear();
                            errorString = string.Format("错误：描述文件中的文件名重复 {0}", filePath);
                            return false;
                        }

                        LangContent langContent = new LangContent();
                        langContent.FileName = fileName;
                        langContent.FileAbbr = fileAbbr;

                        XmlReader subReader = reader.ReadSubtree();
                        while (subReader.Read())
                        {
                            if (subReader.Name == "Field" && subReader.NodeType == XmlNodeType.Element)
                            {
                                LangField field = new LangField();
                                field.FieldName = reader.GetAttribute("Name");
                                field.FieldAbbr = reader.GetAttribute("Abbr");

                                langContent.LangFields.Add(field);

                                Utils.Log(string.Format("LangField: path={0} file={1} field={2}", filePath, fileName, field.FieldName));
                            }
                        }
                        m_FilePaths.Add(filePath);
                        m_Description.Add(fileName, langContent);
                    }
                }
            }
        }

        errorString = null;
        return true;
    }



}
