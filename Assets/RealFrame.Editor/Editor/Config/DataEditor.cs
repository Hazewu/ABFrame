using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.Reflection;
using System.ComponentModel;
using Packages.Rider.Editor.Debugger;

public class DataEditor
{
    private static string Xml_Path = "Assets/GameData/ConfigData/Xml/";
    private static string Binary_Path = "Assets/GameData/ConfigData/Binary/";
    private static string Script_Path = "Assets/Scripts/Data";
    private static string Excel_Path = Application.dataPath + "/../Data/Excel/";
    private static string Reg_Path = Application.dataPath + "/../Data/Reg/";


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

    [MenuItem("Tools/Xml/Xml转二进制")]
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

    [MenuItem("Tools/Xml/Excel转xml")]
    public static void AllExcelToXml()
    {
        string[] filePaths = Directory.GetFiles(Reg_Path, "*", SearchOption.AllDirectories);
        int length = filePaths.Length;
        for (int i = 0; i < length; i++)
        {
            string path = filePaths[i];
            if (!path.EndsWith(".xml"))
                continue;
            EditorUtility.DisplayProgressBar("查找文件夹下的类", "正在扫描路径" + path + "......",
                1.0f / length * i);
            string name = path.Substring(path.LastIndexOf("/") + 1);
            ExcelToXml(name.Replace(".xml", ""));
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

    [MenuItem("Assets/xml转excel")]
    public static void AssetsXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的xml转excel", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            XmlToExcel(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    private static void XmlToExcel(string name)
    {
        string excelName = "";
        string xmlName = "";
        string className = "";

        // 存储所有变量的表
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);
        if (allSheetClassDic == null)
        {
            return;
        }
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();

        object data = GetObjFromXml(className);

        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (SheetClass sheetClass in allSheetClassDic.Values)
        {
            if (sheetClass.Depth == 1)
            {
                outSheetList.Add(sheetClass);
            }
        }

        for (int i = 0; i < outSheetList.Count; i++)
        {
            ReadData(data, outSheetList[i], allSheetClassDic, sheetDataDic);
        }

        // 准备写入excel
        string xlsxPath = Excel_Path + excelName + ".xlsx";
        if (FileIsUsed(xlsxPath))
        {
            Debug.LogError("文件被占用，无法修改");
            return;
        }
        else
        {
            try
            {
                FileInfo xlsxFile = new FileInfo(xlsxPath);
                if (xlsxFile.Exists)
                {
                    xlsxFile.Delete();
                    xlsxFile = new FileInfo(xlsxPath);
                }
                using (ExcelPackage package = new ExcelPackage(xlsxFile))
                {
                    foreach (string str in sheetDataDic.Keys)
                    {

                        // 添加sheet
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(str);
                        worksheet.Cells.AutoFitColumns();
                        SheetData sheetData = sheetDataDic[str];
                        // 写入字段名（列）
                        for (int i = 0; i < sheetData.AllNames.Count; i++)
                        {
                            ExcelRange range = worksheet.Cells[1, i + 1];
                            range.Value = sheetData.AllNames[i];
                            range.AutoFitColumns();
                        }

                        // 写入每行的数据
                        for (int i = 0; i < sheetData.AllData.Count; i++)
                        {
                            RowData rowData = sheetData.AllData[i];
                            // 每列
                            for (int j = 0; j < sheetData.AllNames.Count; j++)
                            {
                                ExcelRange range = worksheet.Cells[i + 2, j + 1];
                                string value = rowData.RowDataDic[sheetData.AllNames[j]];
                                range.Value = value;
                                range.AutoFitColumns();
                                if (value.Contains("\n") || value.Contains("\r\n"))
                                {
                                    // 自动换行
                                    range.Style.WrapText = true;
                                }
                            }
                        }
                    }
                    // 保存数据
                    package.Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
            Debug.Log("生成 " + xlsxPath + " 成功!!!");
        }
    }

    private static void ExcelToXml(string name)
    {
        string className = "";
        string xmlName = "";
        string excelName = "";
        // 第一步，读取reg文件，确定类的结构
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);

        // 第二步，读取excel里面的数据
        string excelPath = Excel_Path + excelName + ".xlsx";
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        try
        {
            using (FileStream fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    ExcelWorksheets worksheetArray = package.Workbook.Worksheets;
                    for (int i = 0; i < worksheetArray.Count; i++)
                    {
                        SheetData sheetData = new SheetData();
                        ExcelWorksheet worksheet = worksheetArray[i + 1];
                        SheetClass sheetClass = allSheetClassDic[worksheet.Name];
                        int colCount = worksheet.Dimension.End.Column;
                        int rowCount = worksheet.Dimension.End.Row;

                        // 读取列名和类型
                        for (int n = 0; n < sheetClass.VarList.Count; n++)
                        {
                            sheetData.AllNames.Add(sheetClass.VarList[n].Name);
                            sheetData.AllTypes.Add(sheetClass.VarList[n].Type);
                        }

                        // 读取数据
                        for (int m = 1; m < rowCount; m++)
                        {
                            RowData rowData = new RowData();
                            for (int n = 0; n < colCount; n++)
                            {
                                ExcelRange range = worksheet.Cells[m + 1, n + 1];
                                string value = "";
                                if (range.Value != null)
                                {
                                    value = range.Value.ToString().Trim();
                                }
                                string colName = worksheet.Cells[1, n + 1].Value.ToString().Trim();
                                rowData.RowDataDic.Add(GetNameFromCol(sheetClass.VarList, colName), value);
                            }

                            sheetData.AllData.Add(rowData);
                        }
                        sheetDataDic.Add(worksheet.Name, sheetData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        // 根据类的结构，创建类，并且给每个变量赋值（从excel里读出来的值）
        object objClass = CreateClass(className);

        List<string> outKeyList = new List<string>();
        foreach (string str in allSheetClassDic.Keys)
        {
            SheetClass sheetClass = allSheetClassDic[str];
            if (sheetClass.Depth == 1)
            {
                outKeyList.Add(str);
            }
        }

        for (int i = 0; i < outKeyList.Count; i++)
        {
            ReadDataToClass(objClass, allSheetClassDic[outKeyList[i]], sheetDataDic[outKeyList[i]],
                allSheetClassDic, sheetDataDic);
        }

        // 序列化
        BinarySerializeOpt.XmlSerialize(Xml_Path + xmlName, objClass);
        Debug.Log(excelName + "表导入unity完成!");
        AssetDatabase.Refresh();
    }

    private static void ReadDataToClass(object objClass, SheetClass sheetClass, SheetData sheetData,
        Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic)
    {
        // 只是为了得到变量类型
        object item = CreateClass(sheetClass.Name);
        object list = CreateList(item.GetType());

        for (int i = 0; i < sheetData.AllData.Count; i++)
        {
            object addItem = CreateClass(sheetClass.Name);
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                VarClass varClass = sheetClass.VarList[j];
                if (varClass.Type == "list" && string.IsNullOrEmpty(varClass.SplitStr))
                {
                    ReadDataToClass(addItem, allSheetClassDic[varClass.ListName], sheetDataDic[varClass.ListName],
                        allSheetClassDic, sheetDataDic);
                }
                else if (varClass.Type == "list")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllNames[j]];
                    SetSplitClassList(addItem, allSheetClassDic[varClass.ListSheetName], value);
                }
                else if (varClass.Type == "listStr")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllNames[j]];
                    SetSplitBaseList(addItem, varClass, value);
                }
                else
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllNames[j]];
                    if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DefaultValue))
                    {
                        value = varClass.DefaultValue;
                    }
                    if (string.IsNullOrEmpty(value))
                    {
                        Debug.LogError("表格有空数据，或者reg文件未配置defaultValue！" + sheetData.AllNames[j]);
                        continue;
                    }
                    SetPropertyValue(addItem, sheetData.AllNames[j], sheetData.AllTypes[j], value);
                }
            }
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, list, new object[] { addItem });
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 自定义类list赋值
    /// </summary>
    /// <param name="objClass"></param>
    /// <param name="sheetClass"></param>
    /// <param name="value"></param>
    private static void SetSplitClassList(object objClass, SheetClass sheetClass, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log("excel里面自定义list的列有空值!" + sheetClass.Name);
            return;
        }
        object item = CreateClass(sheetClass.Name);
        object list = CreateList(item.GetType());

        string splitStr = sheetClass.ParentVar.SplitStr;
        splitStr = splitStr.Replace("\\n", "\n").Replace("\\r", "\r");
        string[] rowArray = value.Split(new string[] { splitStr }, StringSplitOptions.None);
        for (int i = 0; i < rowArray.Length; i++)
        {
            object addItem = CreateClass(sheetClass.Name);
            string[] valueList = rowArray[i].Trim().Split(new string[] { sheetClass.SplitStr }, StringSplitOptions.None);
            for (int j = 0; j < valueList.Length; j++)
            {
                SetPropertyValue(addItem, sheetClass.VarList[j].Name, sheetClass.VarList[j].Type, valueList[j]);
            }
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, list, new object[] { addItem });
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }

    private static void SetSplitBaseList(object objClass, VarClass varClass, string value)
    {
        Type type = null;
        if (varClass.Type == "listStr")
        {
            type = typeof(string);
        }
        object list = CreateList(type);
        string[] rowArray = value.Split(new string[] { varClass.SplitStr }, StringSplitOptions.None);
        for (int i = 0; i < rowArray.Length; i++)
        {
            object addItem = rowArray[i];
            try
            {
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                    null, list, new object[] { addItem });
            }
            catch
            {
                Debug.LogError(varClass.ListSheetName + " 里 " + varClass.Name + " 列表添加失败！具体数值是：" + addItem);
            }
        }
        objClass.GetType().GetProperty(varClass.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 根据列名获得变量名
    /// </summary>
    /// <param name="varList"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string GetNameFromCol(List<VarClass> varList, string col)
    {
        foreach (VarClass varClass in varList)
        {
            if (varClass.Col == col)
                return varClass.Name;
        }
        return null;
    }
    /// <summary>
    /// 读取reg配置
    /// </summary>
    /// <param name="name"></param>
    /// <param name="excelName"></param>
    /// <param name="xmlName"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    private static Dictionary<string, SheetClass> ReadReg(string name, ref string excelName,
        ref string xmlName, ref string className)
    {
        string regPath = Reg_Path + name + ".xml";
        if (!File.Exists(regPath))
        {
            Debug.LogError("此数据不存在配置变化xml:" + name);
            return null;
        }

        XmlDocument xml = new XmlDocument();
        XmlReader reader = XmlReader.Create(regPath);
        XmlReaderSettings settings = new XmlReaderSettings();
        // 忽略xml里面的注释
        settings.IgnoreComments = true;
        xml.Load(reader);

        // data层
        XmlNode xn = xml.SelectSingleNode("data");
        XmlElement xe = (XmlElement)xn;
        className = xe.GetAttribute("name");
        xmlName = xe.GetAttribute("to");
        excelName = xe.GetAttribute("from");

        // 存储所有变量的表
        Dictionary<string, SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
        ReadXmlNode(xe, allSheetClassDic, 0);
        reader.Close();
        return allSheetClassDic;
    }

    /// <summary>
    /// 递归读取配置
    /// </summary>
    /// <param name="xmlElement"></param>
    private static void ReadXmlNode(XmlElement xmlElement,
        Dictionary<string, SheetClass> allSheetClassDic, int depth)
    {
        depth++;
        foreach (XmlNode node in xmlElement.ChildNodes)
        {
            XmlElement xe = (XmlElement)node;
            if (xe.GetAttribute("type") == "list")
            {

                // 创建父级变量
                VarClass parentVar = new VarClass()
                {
                    Name = xe.GetAttribute("name"),
                    Type = xe.GetAttribute("type"),
                    Col = xe.GetAttribute("col"),
                    DefaultValue = xe.GetAttribute("defaultValue"),
                    Foreign = xe.GetAttribute("foreign"),
                    SplitStr = xe.GetAttribute("split")
                };
                // 创建sheet对象
                XmlElement listElem = (XmlElement)node.FirstChild;
                // 这里也存疑TODO
                if (parentVar.Type == "list")
                {
                    parentVar.ListName = listElem.GetAttribute("name");
                    parentVar.ListSheetName = listElem.GetAttribute("sheetname");
                }

                SheetClass sheetClass = new SheetClass()
                {
                    Name = listElem.GetAttribute("name"),
                    SheetName = listElem.GetAttribute("sheetname"),
                    MainKey = listElem.GetAttribute("mainKey"),
                    SplitStr = listElem.GetAttribute("split"),
                    ParentVar = parentVar,
                    Depth = depth
                };

                // 怎么感觉这里不太对TODO，不是递归吗
                if (!string.IsNullOrEmpty(sheetClass.SheetName) && !allSheetClassDic.ContainsKey(sheetClass.SheetName))
                {
                    // 获取该类下面所有变量
                    foreach (XmlNode insideNode in listElem.ChildNodes)
                    {
                        XmlElement insideXe = (XmlElement)insideNode;
                        VarClass varClass = new VarClass()
                        {
                            Name = insideXe.GetAttribute("name"),
                            Type = insideXe.GetAttribute("type"),
                            Col = insideXe.GetAttribute("col"),
                            DefaultValue = insideXe.GetAttribute("defaultValue"),
                            Foreign = insideXe.GetAttribute("foreign"),
                            SplitStr = insideXe.GetAttribute("split")
                        };
                        if (varClass.Type == "list")
                        {
                            XmlElement insideListElem = (XmlElement)insideXe.FirstChild;
                            varClass.ListName = insideListElem.GetAttribute("name");
                            varClass.ListSheetName = insideListElem.GetAttribute("sheetname");
                        }

                        sheetClass.VarList.Add(varClass);
                    }
                    allSheetClassDic.Add(sheetClass.SheetName, sheetClass);
                }

                // 不需要了吧？？
                ReadXmlNode(listElem, allSheetClassDic, depth);
            }
        }
    }

    /// <summary>
    /// 读取表中的数据，TODO，不建议做表嵌套，没必要让事情变复杂，后续优化
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sheetClass"></param>
    /// <param name="allSheetClassDic"></param>
    /// <param name="sheetDataDic"></param>
    private static void ReadData(object data, SheetClass sheetClass,
        Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic)
    {
        List<VarClass> varList = sheetClass.VarList;
        VarClass parentVar = sheetClass.ParentVar;
        object dataList = GetMemberValue(data, parentVar.Name);

        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default
            | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        SheetData sheetData = new SheetData();
        // 遍历列
        for (int i = 0; i < varList.Count; i++)
        {
            if (!string.IsNullOrEmpty(varList[i].Col))
            {
                sheetData.AllNames.Add(varList[i].Col);
                sheetData.AllTypes.Add(varList[i].Type);
            }
        }

        // 有多少行数据
        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, dataList, new object[] { i });

            RowData rowData = new RowData();

            // 每行数据有多少列
            for (int j = 0; j < varList.Count; j++)
            {
                VarClass colVar = varList[j];
                // 新加表
                if (colVar.Type == "list" && string.IsNullOrEmpty(colVar.SplitStr))
                {
                    SheetClass tempSheetClass = allSheetClassDic[colVar.ListSheetName];
                    ReadData(item, tempSheetClass, allSheetClassDic, sheetDataDic);
                }
                // list+自定义类
                else if (colVar.Type == "list")
                {
                    SheetClass tempSheetClass = allSheetClassDic[colVar.ListSheetName];
                    string value = GetSplitClassList(item, colVar, tempSheetClass);
                    rowData.RowDataDic.Add(colVar.Col, value);
                }
                // list+基础类型
                else if (colVar.Type == "listStr")
                {
                    string value = GetSplitBaseList(item, colVar);
                    rowData.RowDataDic.Add(colVar.Col, value);
                }
                else
                {
                    object value = GetMemberValue(item, colVar.Name);
                    if (value != null)
                    {
                        rowData.RowDataDic.Add(colVar.Col, value.ToString());
                    }
                    else
                    {
                        Debug.LogError(varList[j].Name + " 反射出来为空!");
                    }
                }
            }

            string key = parentVar.ListSheetName;
            if (sheetDataDic.ContainsKey(key))
            {
                sheetDataDic[key].AllData.Add(rowData);
            }
            else
            {
                sheetData.AllData.Add(rowData);
                sheetDataDic.Add(key, sheetData);
            }
        }

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
    /// 获取基础list里面的所有值
    /// </summary>
    /// <param name="data"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetSplitBaseList(object data, VarClass varClass)
    {
        string str = "";
        if (string.IsNullOrEmpty(varClass.SplitStr))
        {
            Debug.LogError("基础list的分隔符为空！");
            return str;
        }
        object dataList = GetMemberValue(data, varClass.Name);
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default
            | BindingFlags.InvokeMethod, null, dataList, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
            str += item.ToString();
            if (i != listCount - 1)
            {
                str += varClass.SplitStr;
            }
        }
        return str;
    }

    /// <summary>
    /// 获取类list里面的所有值
    /// </summary>
    /// <param name="data"></param>
    /// <param name="varClass"></param>
    /// <param name="sheetClass"></param>
    /// <returns></returns>
    private static string GetSplitClassList(object data, VarClass varClass, SheetClass sheetClass)
    {
        string split = varClass.SplitStr;
        string classSplit = sheetClass.SplitStr;
        string str = "";
        if (string.IsNullOrEmpty(split) || string.IsNullOrEmpty(classSplit))
        {
            Debug.LogError("类的列类分隔符或变量分隔符为空！！！");
            return str;
        }
        classSplit = classSplit.Replace("\\n", "\n").Replace("\\r", "\r");
        split = split.Replace("\\n", "\n").Replace("\\r", "\r");

        object dataList = GetMemberValue(data, varClass.Name);
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default
            | BindingFlags.InvokeMethod, null, dataList, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                object value = GetMemberValue(item, sheetClass.VarList[j].Name);
                str += value.ToString();
                if (j != sheetClass.VarList.Count - 1)
                {
                    str += classSplit;
                }
            }
            if (i != listCount - 1)
            {
                str += varClass.SplitStr;
            }
        }
        return str;
    }

    [MenuItem("Tools/测试/测试读取xml")]
    public static void TextReadXml()
    {
        string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
        XmlReader reader = null;
        try
        {
            XmlDocument xml = new XmlDocument();
            reader = XmlReader.Create(xmlPath);
            xml.Load(reader);

            XmlNode root = xml.SelectSingleNode("data");
            XmlElement xe = (XmlElement)root;

            string className = xe.GetAttribute("name");
            string xmlName = xe.GetAttribute("to");
            string excelName = xe.GetAttribute("from");
            reader.Close();
            Debug.Log(className + " " + xmlName + " " + excelName);

            foreach (XmlNode node in xe.ChildNodes)
            {
                // variable层
                XmlElement tempXe = (XmlElement)node;
                string name = tempXe.GetAttribute("name");
                string type = tempXe.GetAttribute("type");
                Debug.Log(name + " " + type);
                // list层
                XmlNode listNode = tempXe.FirstChild;
                XmlElement listElem = (XmlElement)listNode;
                string listName = listElem.GetAttribute("name");
                string sheetName = listElem.GetAttribute("sheetname");
                string mainkey = listElem.GetAttribute("mainkey");
                Debug.Log("list:" + listName + " " + sheetName + " " + mainkey);
                // variable层
                foreach (XmlNode nd in listElem.ChildNodes)
                {
                    XmlElement txe = (XmlElement)nd;
                    Debug.Log(txe.GetAttribute("name") + " " + txe.GetAttribute("col") + " " + txe.GetAttribute("type"));
                }
            }
        }
        catch (Exception e)
        {
            if (reader != null)
            {
                reader.Close();
            }
            Debug.LogError(e);
        }
    }

    [MenuItem("Tools/测试/测试写入excel")]
    public static void TextWriteExcel()
    {
        string xlsxPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
        FileInfo xlsxFile = new FileInfo(xlsxPath);
        if (xlsxFile.Exists)
        {
            xlsxFile.Delete();
            xlsxFile = new FileInfo(xlsxPath);
        }
        using (ExcelPackage package = new ExcelPackage(xlsxFile))
        {
            ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("怪物配置");

            ExcelRange range = workSheet.Cells[1, 1];
            range.Value = "测试sssssssssssssssdddd";
            range.AutoFitColumns();
            range.Style.WrapText = true;

            package.Save();
            Debug.Log("写入成功");
        }
    }

    [MenuItem("Tools/测试/测试已有类反射为数据")]
    public static void TestReflection1()
    {
        TestInfo testInfo = new TestInfo()
        {
            Id = 2,
            Name = "赠汪伦",
            IsA = false,
            AllStrList = new List<string>(),
            AllTestInfoList = new List<TestInfoTwo>()
        };
        testInfo.AllStrList.Add("李白乘舟将欲行");
        testInfo.AllStrList.Add("忽闻岸上踏歌声");
        testInfo.AllStrList.Add("桃花潭水深千尺");
        testInfo.AllStrList.Add("不及汪伦送我情");

        for (int i = 0; i < 3; i++)
        {
            TestInfoTwo test = new TestInfoTwo();
            test.Id = i;
            test.Name = i + " name";
            testInfo.AllTestInfoList.Add(test);
        }

        Debug.LogError("Id:" + GetMemberValue(testInfo, "Id"));
        Debug.LogError("Name:" + GetMemberValue(testInfo, "Name"));
        Debug.LogError("IsA:" + GetMemberValue(testInfo, "IsA"));

        // 简单基础类型的list反射
        object list = GetMemberValue(testInfo, "AllStrList");
        int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default
            | BindingFlags.InvokeMethod, null, list, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, list, new object[] { i });
            Debug.LogError("list [" + i + "] : " + item);
        }

        // 自定义类型的list反射
        object infoList = GetMemberValue(testInfo, "AllTestInfoList");
        int infoListCount = System.Convert.ToInt32(infoList.GetType().InvokeMember("get_Count", BindingFlags.Default
            | BindingFlags.InvokeMethod, null, infoList, new object[] { }));
        for (int i = 0; i < infoListCount; i++)
        {
            object item = infoList.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, infoList, new object[] { i });
            object id = GetMemberValue(item, "Id");
            object name = GetMemberValue(item, "Name");
            Debug.LogError("list [" + id + "] : " + name);
        }
    }

    [MenuItem("Tools/测试/测试已有数据反射为类")]
    public static void TestRefection2()
    {
        object obj = CreateClass("TestInfo");

        SetPropertyValue(obj, "Id", "int", "20");
        SetPropertyValue(obj, "Name", "string", "静夜诗");
        SetPropertyValue(obj, "IsA", "bool", "true");
        SetPropertyValue(obj, "Height", "float", "51.4");
        SetPropertyValue(obj, "TestType", "enum", "VAR1");

        object list = CreateList(typeof(string));
        for (int i = 0; i < 3; i++)
        {
            object addItem = "测试填数据" + i;
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, list, new object[] { addItem }); // 调用list的add方法添加数据
        }

        obj.GetType().GetProperty("AllStrList").SetValue(obj, list);


        object twoList = CreateList(typeof(TestInfoTwo));
        for (int i = 0; i < 3; i++)
        {
            object addItem = CreateClass("TestInfoTwo");
            SetPropertyValue(addItem, "Id", "int", "150" + i);
            SetPropertyValue(addItem, "Name", "string", "测试类" + i);
            twoList.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, twoList, new object[] { addItem }); // 调用list的add方法添加数据
        }

        obj.GetType().GetProperty("AllTestInfoList").SetValue(obj, twoList);

        TestInfo testInfo = obj as TestInfo;
        Debug.LogError(testInfo.Id + " " + testInfo.Name + " " + testInfo.IsA + " "
            + testInfo.Height + " " + testInfo.TestType);
        foreach (string str in testInfo.AllStrList)
        {
            Debug.Log(str);
        }
        foreach (TestInfoTwo test in testInfo.AllTestInfoList)
        {
            Debug.Log(test.Id + " " + test.Name);
        }
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
}

public class TestInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsA { get; set; }

    public float Height { get; set; }

    public TestEnum TestType { get; set; }

    public List<string> AllStrList { get; set; }
    public List<TestInfoTwo> AllTestInfoList { get; set; }
}

public class TestInfoTwo
{
    public int Id { get; set; }
    public string Name { set; get; }
}

public enum TestEnum
{
    None = 0,
    VAR1 = 1,
    TEST2 = 2
}

/// <summary>
/// 变量中间类
/// </summary>
public class VarClass
{
    // 原类里面变量的名称
    public string Name { get; set; }
    // 变量类型
    public string Type { get; set; }
    // 变量对应的excel里面的列
    public string Col { get; set; }
    // 变量的默认值
    public string DefaultValue { get; set; }
    // 变量是list的话，外联部分列，即外键
    public string Foreign { get; set; }
    // 分隔符
    public string SplitStr { get; set; }
    // 如果自己是List，对应的list类名
    public string ListName { get; set; }
    // 如果自己是list，对应的sheet名
    public string ListSheetName { get; set; }
}

/// <summary>
/// sheet中间类
/// </summary>
public class SheetClass
{
    // sheet所属父级的var变量
    public VarClass ParentVar { get; set; }
    // 深度，用来确定是第几个list
    public int Depth { get; set; }
    // 类名
    public string Name { get; set; }
    // 类对应的sheet名
    public string SheetName { get; set; }
    // 主键
    public string MainKey { get; set; }
    // 分隔符
    public string SplitStr { get; set; }
    // 所包含的变量
    public List<VarClass> VarList = new List<VarClass>();
}

/// <summary>
/// 表中的数据
/// </summary>
public class SheetData
{
    // 所有列的名字
    public List<string> AllNames = new List<string>();
    // 所有列的类型
    public List<string> AllTypes = new List<string>();
    // 所有行数据
    public List<RowData> AllData = new List<RowData>();
}

/// <summary>
/// 存储每一行中的数据
/// </summary>
public class RowData
{
    // key=列名，value=数据
    public Dictionary<string, string> RowDataDic = new Dictionary<string, string>();
}