using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class BuffData : ExcelBase
{
    [XmlElement("BuffDataList")]
    public List<BuffDataBase> BuffDataList { get; set; }

    [XmlIgnore]
    public Dictionary<int, BuffDataBase> AllBuffDic = new Dictionary<int, BuffDataBase>();

#if UNITY_EDITOR
    public override void Construction()
    {
        BuffDataList = new List<BuffDataBase>();
        for (int i = 0; i < 10; i++)
        {
            BuffDataBase buff = new BuffDataBase();
            buff.Id = i + 1;
            buff.Name = "BUFF" + i;
            buff.OutLook = "Assets/GameData/..." + i;
            buff.Time = Random.Range(0.5f, 10);
            buff.BuffType = (BuffEnum)Random.Range(0, 4);
            buff.AllString = new List<string>();
            buff.AllString.Add("ceshi" + i);
            buff.AllString.Add("run" + i);
            BuffDataList.Add(buff);
        }
    }
#endif
    public override void Init()
    {
        AllBuffDic.Clear();
        for (int i = 0; i < BuffDataList.Count; i++)
        {
            AllBuffDic.Add(BuffDataList[i].Id, BuffDataList[i]);
        }
    }

    /// <summary>
    /// 根据Id查找buff
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BuffDataBase FindBuffById(int id)
    {
        return AllBuffDic[id];
    }
}

[System.Serializable]
public class BuffDataBase
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
    [XmlAttribute("OutLook")]
    public string OutLook { get; set; }
    [XmlAttribute("Time")]
    public float Time { get; set; }
    [XmlAttribute("BuffType")]
    public BuffEnum BuffType { get; set; }
    [XmlElement("AllString")]
    public List<string> AllString { get; set; }
}



public enum BuffEnum
{
    Node = 0,
    Fire = 1,
    Ice = 2,
    Poison = 3
}
