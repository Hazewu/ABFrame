using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class BuffData : ExcelBase
{
    [XmlElement("AllBuffList")]
    public List<BuffBase> AllBuffList { get; set; }

    [XmlElement("MonsterBuffList")]
    public List<BuffBase> MonsterBuffList { get; set; }

    [XmlIgnore]
    public Dictionary<int, BuffBase> AllBuffDic = new Dictionary<int, BuffBase>();
    [XmlIgnore]
    public Dictionary<int, BuffBase> MonsterBuffDic = new Dictionary<int, BuffBase>();

    public override void Construction()
    {
        AllBuffList = new List<BuffBase>();
        for (int i = 0; i < 10; i++)
        {
            BuffBase buff = new BuffBase();
            buff.Id = i + 1;
            buff.Name = "BUFF" + i;
            buff.OutLook = "Assets/GameData/..." + i;
            buff.Time = Random.Range(0.5f, 10);
            buff.BuffType = (BuffEnum)Random.Range(0, 4);
            AllBuffList.Add(buff);
        }
        MonsterBuffList = new List<BuffBase>();
        for (int i = 0; i < 5; i++)
        {
            BuffBase buff = new BuffBase();
            buff.Id = i + 1;
            buff.Name = "����BUFF" + i;
            buff.OutLook = "Assets/GameData/Monster/..." + i;
            buff.Time = Random.Range(0.5f, 10);
            buff.BuffType = (BuffEnum)Random.Range(0, 4);
            MonsterBuffList.Add(buff);
        }
    }

    public override void Init()
    {
        AllBuffDic.Clear();
        MonsterBuffDic.Clear();
        for (int i = 0; i < AllBuffList.Count; i++)
        {
            AllBuffDic.Add(AllBuffList[i].Id, AllBuffList[i]);
        }
        for (int i = 0; i < MonsterBuffList.Count; i++)
        {
            MonsterBuffDic.Add(MonsterBuffList[i].Id, MonsterBuffList[i]);
        }
    }

    /// <summary>
    /// ����Id����buff
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BuffBase FindBuffById(int id)
    {
        return AllBuffDic[id];
    }

    /// <summary>
    /// ����id���ҹ���buff
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BuffBase FindMonsterBuffById(int id)
    {
        return MonsterBuffDic[id];
    }
}

[System.Serializable]
public class BuffBase
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
}

public enum BuffEnum
{
    Node = 0,
    Fire = 1,
    Ice = 2,
    Poison = 3
}
