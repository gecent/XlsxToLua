using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public struct LangField
{
    public string FieldName; // Read from Description
    public string FieldOldName; // Read from Description
}

public class LangDescriptionReader
{
    private List<string> m_FilePaths = new List<string>();
    private Dictionary<String, List<LangField>> m_Description = new Dictionary<string, List<LangField>>();

    /// <summary>
    /// key: 文件名（去掉/之前的），value: 文件中的字段
    /// </summary>
    public Dictionary<String, List<LangField>> Description { get { return m_Description; } private set { Description = value; } }

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

                        XmlReader subReader = reader.ReadSubtree();
                        List<LangField> listField = new List<LangField>();
                        while (subReader.Read())
                        {
                            if (subReader.Name == "Field" && subReader.NodeType == XmlNodeType.Element)
                            {
                                LangField field = new LangField();
                                field.FieldName = reader.GetAttribute("Name");
                                field.FieldOldName = reader.GetAttribute("OldName");

                                listField.Add(field);

                                Utils.Log(string.Format("LangField: path={0} file={1} field={2}", filePath, fileName, field.FieldName));
                            }
                        }
                        m_FilePaths.Add(filePath);
                        m_Description.Add(fileName, listField);
                    }
                }
            }
        }

        errorString = null;
        return true;
    }



}
