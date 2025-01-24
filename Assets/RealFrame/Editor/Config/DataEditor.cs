using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.Reflection;
using System.ComponentModel;

/// <summary>
/// 把value转换为特定类型的值的方法
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
public delegate object OnConvertValue(string value);

public class DataEditor
{
    private static string Xml_Path = RealConfig.GetConfig().m_XmlPath;
    private static string Binary_Path = RealConfig.GetConfig().m_BinaryPath;
    private static string Excel_Path = Application.dataPath + RealConfig.GetConfig().m_ExcelPath;

    // key=类型字符串，value=方法
    private static Dictionary<string, OnConvertValue> m_ConvertDic = new Dictionary<string, OnConvertValue>();
    // key=类型字符串，value=type
    private static Dictionary<string, Type> m_BaseTypeDic = new Dictionary<string, Type>();
    private static bool m_InitConvertDic = false;


    [MenuItem("Assets/类转xml")]
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的类转成xml", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 提供单个类转xml方法
    /// </summary>
    /// <param name="name"></param>
    private static void ClassToXml(string name)
    {
        try
        {
            // 需要获取当前主程序的所有程序集，根据name找到对应的类进行实例化
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                var temp = Activator.CreateInstance(type);
                if (temp is ExcelBase)
                {
                    (temp as ExcelBase).Construction();
                }
                string xmlPath = Xml_Path + name + ".xml";
                BinarySerializeOpt.XmlSerialize(xmlPath, temp);
                Debug.Log(name + "类转xml成功，xml路径为:" + xmlPath);
            }
        }
        catch
        {
            Debug.LogError(name + "类转xml失败!");
        }
    }

    [MenuItem("Assets/Xml转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转二进制", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/导表/Xml转Binary")]
    public static void AllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + Xml_Path;
        string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        int length = filesPath.Length;
        for (int i = 0; i < length; i++)
        {
            string tempPath = filesPath[i];
            EditorUtility.DisplayProgressBar("查找文件夹下面的Xml", "正在扫描" + tempPath + "......", 1.0f / length * i);
            if (tempPath.EndsWith(".xml"))
            {
                string name = tempPath.Substring(tempPath.LastIndexOf("/") + 1);
                name = name.Replace(".xml", "");
                XmlToBinary(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// xml转二进制
    /// </summary>
    /// <param name="name"></param>
    private static void XmlToBinary(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                string xmlPath = Xml_Path + name + ".xml";
                string binaryPath = Binary_Path + name + ".bytes";
                object obj = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
                BinarySerializeOpt.BinarySerialize(binaryPath, obj);
                Debug.Log(name + " xml转二进制成功，二进制路径为：" + binaryPath);
            }
        }
        catch
        {
            Debug.LogError(name + " xml转二进制失败!");
        }
    }

    [MenuItem("Tools/导表/Excel转Xml")]
    public static void ExcelToXmlNew()
    {
        InitConvertDic();
        string excelName = "Poetry_古诗";
        string excelPath = Excel_Path + "Poetry_古诗.xlsx";
        string className = "Poetry";
        string xmlName = "Poetry.xml";
        // 第一步，打开excel
        // 第二步，读取1234行数据，获取字段属性
        // 第三步，读取数据
        HaSheetData sheetData = ReadExcelData(excelPath);
        if (sheetData == null)
        {
            return;
        }

        // 第四步，根据类的结构，创建类，并且给每个变量赋值
        object objClass = CreateClass(className);
        string dataBaseName = className + "Base";
        string dataListName = className + "List";
        ReadDataToClass(objClass, dataBaseName, dataListName, sheetData);

        // 第五步，序列化
        BinarySerializeOpt.XmlSerialize(Xml_Path + xmlName, objClass);
        Debug.Log(excelName + "表导入完成！");
    }

    /// <summary>
    /// 在Excel中读取所有需要的数据
    /// </summary>
    /// <param name="excelPath"></param>
    /// <returns></returns>
    private static HaSheetData ReadExcelData(string excelPath)
    {
        try
        {
            using (FileStream fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // 注意excel中索引从1开始
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    // 只读取第一个表的数据
                    HaSheetData sheetData = new HaSheetData();
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int colCount = worksheet.Dimension.End.Column;
                    int rowCount = worksheet.Dimension.End.Row;
                    if (rowCount < 5)
                    {
                        Debug.LogWarning(excelPath + "表中数据不足5行，没有数据导出失败！");
                        return null;
                    }

                    // 读取列属性，前4行
                    for (int col = 1; col < colCount + 1; col++)
                    {
                        ExcelRange typeRange = worksheet.Cells[1, col];
                        ExcelRange nameRange = worksheet.Cells[2, col];
                        string name = nameRange.Value.ToString().Trim();
                        string type = typeRange.Value.ToString().Trim();
                        bool isArray = false;
                        if (type.Contains("[]"))
                        {
                            isArray = true;
                            type = type.Replace("[]", "");
                        }
                        HaColProperty property = new HaColProperty()
                        {
                            Name = name,
                            Type = type,
                            IsArray = isArray,
                        };
                        sheetData.AllCols.Add(property);
                        Debug.Log("name:" + property.Name + " type:" + property.Type + " isArray:" + property.IsArray);
                    }

                    // 读取数据，从第5行开始
                    for (int row = 5; row < rowCount + 1; row++)
                    {
                        HaRowData rowData = new HaRowData();
                        bool isValidData = true;
                        for (int col = 1; col < colCount + 1; col++)
                        {
                            ExcelRange range = worksheet.Cells[row, col];
                            string value = "";
                            if (range.Value != null)
                            {
                                value = range.Value.ToString().Trim();
                            }
                            // 第一列中的数据为空，不读取该行数据
                            if (col == 1 && string.IsNullOrEmpty(value))
                            {
                                isValidData = false;
                                break;
                            }

                            string colName = worksheet.Cells[2, col].Value.ToString().Trim();
                            rowData.DataDic.Add(colName, value);
                        }
                        if (isValidData)
                        {
                            sheetData.AllData.Add(rowData);
                        }
                    }

                    for (int i = 0; i < sheetData.AllData.Count; i++)
                    {
                        string str = i + ":";
                        foreach (string key in sheetData.AllData[i].DataDic.Keys)
                        {
                            str += key + ":" + sheetData.AllData[i].DataDic[key] + "   ";
                        }
                        Debug.Log(str);
                    }
                    return sheetData;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(excelPath + "导表失败！原因：" + e);
            return null;
        }
    }

    private static void ReadDataToClass(object objClass, string className, string objListName, HaSheetData sheetData)
    {
        // 只是为了得到变量类型
        object item = CreateClass(className);
        object list = CreateList(item.GetType());

        // 把每一行数据读到objClass的list中
        for (int i = 0; i < sheetData.AllData.Count; i++)
        {
            object addItem = CreateClass(className);
            // 读取每一列
            for (int j = 0; j < sheetData.AllCols.Count; j++)
            {
                HaColProperty property = sheetData.AllCols[j];
                // 数组
                if (property.IsArray)
                {
                    string value = sheetData.AllData[i].DataDic[property.Name];
                    SetSplitBaseList(addItem, property, value);
                }
                else
                {
                    string value = sheetData.AllData[i].DataDic[property.Name];
                    SetPropertyValue(addItem, property.Name, property.Type, value);
                }
            }
            // 加入到list中
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, list, new object[] { addItem });
        }

        // 把赋值好的list，放入objClass中
        objClass.GetType().GetProperty(objListName).SetValue(objClass, list);
    }

    private static void SetSplitBaseList(object objClass, HaColProperty property, string value)
    {
        Type type = GetBaseType(property.Type);
        if (type == null)
        {
            Debug.LogError(property.Name + "字段的" + property.Type + " 类型无法转换");
            return;
        }
        object list = CreateList(type);
        string[] rowArray = value.Split(new string[] { "|" }, StringSplitOptions.None);
        for (int i = 0; i < rowArray.Length; i++)
        {
            object addItem = GetConvertValue(property.Type, rowArray[i]);
            try
            {
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                    null, list, new object[] { addItem });
            }
            catch
            {
                Debug.LogError(property.Name + " 列表添加失败！具体数值是：" + addItem);
            }
        }
        objClass.GetType().GetProperty(property.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 判断文件是否被占用
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool FileIsUsed(string path)
    {
        bool result = false;
        if (!File.Exists(path))
        {
            result = false;
        }
        else
        {
            FileStream fs = null;
            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                result = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                result = true;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 根据xml的名字（不包含后缀）获得obj，要求obj与xml名字相同
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object GetObjFromXml(string name)
    {
        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(name);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }
        if (type != null)
        {
            // 这里是读的配置数据xml，而不是reg数据xml
            string xmlPath = Xml_Path + name + ".xml";
            return BinarySerializeOpt.XmlDeserialize(xmlPath, type);
        }
        return null;
    }

    /// <summary>
    /// 反射类里面的变量的具体数值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="memberName"></param>
    /// <returns></returns>
    private static object GetMemberValue(object obj, string memberName)
    {
        Type type = obj.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        MemberInfo[] members = type.GetMember(memberName, flags);
        switch (members[0].MemberType)
        {
            case MemberTypes.Field:
                return type.GetField(memberName, flags).GetValue(obj);
            case MemberTypes.Property:
                return type.GetProperty(memberName, flags).GetValue(obj);
            default:
                return null;
        }
    }

    /// <summary>
    /// 根据类名字，反射创建一个类的实列
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object CreateClass(string name)
    {
        object obj = null;
        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(name);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }
        if (type != null)
        {
            obj = Activator.CreateInstance(type);
        }
        return obj;
    }

    /// <summary>
    /// 根据类型new一个list
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object CreateList(Type type)
    {
        Type listType = typeof(List<>);
        // 确定list<>里面的T的类型
        Type specType = listType.MakeGenericType(new System.Type[] { type });
        // new出来这个list
        return Activator.CreateInstance(specType, new object[] { });
    }

    /// <summary>
    /// 给对象的某个属性，设置某种类型的值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    private static void SetPropertyValue(object obj, string propertyName, string type, string value)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(value)) return;

        PropertyInfo info = obj.GetType().GetProperty(propertyName);
        object val = null;
        switch (type)
        {
            case "int":
                val = System.Convert.ToInt32(value);
                break;
            case "float":
                val = System.Convert.ToSingle(value);
                break;
            case "bool":
                val = System.Convert.ToBoolean(val);
                break;
            case "enum":
                val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(value);
                break;
            default:
                val = value;
                break;
        }
        info.SetValue(obj, val);
    }

    #region 根据类型获得type和对应类型的值

    private static Type GetBaseType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;
        Type type = null;
        if (m_BaseTypeDic.TryGetValue(typeName, out type)) return type;
        return type;
    }

    private static object GetConvertValue(string typeName, string value)
    {
        OnConvertValue func = null;
        if (m_ConvertDic.TryGetValue(typeName, out func))
        {
            return func(value);
        }
        return null;
    }

    private static void InitConvertDic()
    {
        if (m_InitConvertDic) return;
        m_InitConvertDic = true;
        m_ConvertDic.Clear();
        m_ConvertDic.Add("int", GetIntConvert);
        m_ConvertDic.Add("string", GetStringConvert);
        m_ConvertDic.Add("bool", GetBoolConvert);
        m_ConvertDic.Add("float", GetFloatConvert);
        //m_ConvertDic.Add("enum", GetEnumConvert);

        m_BaseTypeDic.Clear();
        m_BaseTypeDic.Add("int", typeof(int));
        m_BaseTypeDic.Add("string", typeof(string));
        m_BaseTypeDic.Add("bool", typeof(bool));
        m_BaseTypeDic.Add("float", typeof(float));
        //m_BaseTypeDic.Add("enum",)
    }

    /// <summary>
    /// 转换成int类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object GetIntConvert(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return System.Convert.ToInt32(value);
        }
        return 0;
    }

    /// <summary>
    /// 转换成string类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object GetStringConvert(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }
        return null;
    }

    /// <summary>
    /// 转换成bool类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object GetBoolConvert(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return System.Convert.ToBoolean(value);
        }
        return false;
    }

    /// <summary>
    ///  转换成bool类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object GetFloatConvert(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return System.Convert.ToSingle(value);
        }
        return 0;
    }

    /// <summary>
    /// 转换成enum对应的类型
    /// </summary>
    /// <param name="value"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static object GetEnumConvert(string value, object obj)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return TypeDescriptor.GetConverter(obj).ConvertFromInvariantString(value);
        }
        return null;
    }
    #endregion
}

/// <summary>
/// 表中的数据
/// </summary>
public class HaSheetData
{
    // 所有列属性（读取1234行获得），key=列属性名
    public List<HaColProperty> AllCols = new List<HaColProperty>();
    // 所有行数据（除了1234行）
    public List<HaRowData> AllData = new List<HaRowData>();
}

/// <summary>
/// 列属性，对应类中的每个属性
/// </summary>
public class HaColProperty
{
    // 属性名
    public string Name { get; set; }
    // 类型【int、string、bool、float、enum】
    public string Type { get; set; }
    // 是否是数组
    public bool IsArray { get; set; }
}

/// <summary>
/// 存储每一行中的数据
/// </summary>
public class HaRowData
{
    // key=属性名，value=数据
    public Dictionary<string, string> DataDic = new Dictionary<string, string>();
}