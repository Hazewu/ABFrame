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

#if UNITY_EDITOR
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
            buff.AllString = new List<string>();
            buff.AllString.Add("ceshi" + i);
            buff.AllString.Add("run" + i);
            buff.AllBuffList = new List<BuffTest>();
            int count = Random.Range(0, 4);
            for (int j = 0; j < count; j++)
            {
                BuffTest test = new BuffTest();
                test.Id = j + Random.Range(0, 5);
                test.Name = "name" + j;
                buff.AllBuffList.Add(test);
            }
            AllBuffList.Add(buff);
        }
        MonsterBuffList = new List<BuffBase>();
        for (int i = 0; i < 5; i++)
        {
            BuffBase buff = new BuffBase();
            buff.Id = i + 1;
            buff.Name = "怪物BUFF" + i;
            buff.OutLook = "Assets/GameData/Monster/..." + i;
            buff.Time = Random.Range(0.5f, 10);
            buff.BuffType = (BuffEnum)Random.Range(0, 4);
            buff.AllString = new List<string>();
            buff.AllString.Add("libai" + i);
            buff.AllString.Add("dufu" + i);
            buff.AllString.Add("taoyuanming" + i);
            buff.AllBuffList = new List<BuffTest>();
            int count = Random.Range(0, 4);
            for (int j = 0; j < count; j++)
            {
                BuffTest test = new BuffTest();
                test.Id = j + Random.Range(0, 5);
                test.Name = "buff" + j;
                buff.AllBuffList.Add(test);
            }
            MonsterBuffList.Add(buff);
        }
    }
#endif
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
    /// 根据Id查找buff
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BuffBase FindBuffById(int id)
    {
        return AllBuffDic[id];
    }

    /// <summary>
    /// 根据id查找怪物buff
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
    [XmlElement("AllString")]
    public List<string> AllString { get; set; }
    [XmlElement("AllBuffList")]
    public List<BuffTest> AllBuffList { get; set; }
}



public enum BuffEnum
{
    Node = 0,
    Fire = 1,
    Ice = 2,
    Poison = 3
}

[System.Serializable]
public class BuffTest
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
}
