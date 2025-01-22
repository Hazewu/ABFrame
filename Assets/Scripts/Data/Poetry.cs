using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Poetry : ExcelBase
{
    // 命名是Poetry+List
    [XmlElement("PoetryList")]
    public List<PoetryBase> PoetryList { get; set; }

    [XmlIgnore]
    public Dictionary<int, PoetryBase> PoetryDic = new Dictionary<int, PoetryBase>();

#if UNITY_EDITOR
    public override void Construction()
    {
        //
    }
#endif

    public override void Init()
    {
        PoetryDic.Clear();
        for (int i = 0; i < PoetryList.Count; i++)
        {
            PoetryDic.Add(PoetryList[i].Id, PoetryList[i]);
        }
    }

    /// <summary>
    /// 根据id查找古诗
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public PoetryBase FindPoetryById(int id)
    {
        return PoetryDic[id];
    }
}

/// <summary>
/// 命名是Poetry+Base
/// </summary>
[System.Serializable]
public class PoetryBase
{
    // 序号
    [XmlAttribute("Id")]
    public int Id { get; set; }
    // 图片对应id数组
    [XmlElement("ImageIds")]
    public List<int> ImageIds { get; set; }
    // 诗名
    [XmlAttribute("Name")]
    public string Name { get; set; }
    // 诗人名字
    [XmlAttribute("PoetName")]
    public string PoetName { get; set; }
    // 诗句
    [XmlElement("Content")]
    public List<string> Content { get; set; }
    // 是否显示
    [XmlAttribute("Show")]
    public bool Show { get; set; }
    // 是否特殊
    [XmlElement("Specials")]
    public List<bool> Specials { get; set; }
    // 诗类型
    [XmlAttribute("PoetType")]
    public EmPoetType PoetType { get; set; }
    // 特效数组
    [XmlElement("Effects")]
    public EmEffectType Effects { get; set; }
}

public enum EmPoetType
{
    // 五言绝句
    Four_Five = 0,
    // 七言绝句
    Four_Seven = 1
}

public enum EmEffectType
{
    Fire = 0,
    Water = 1,
    Light = 2,
    Dark = 3
}
