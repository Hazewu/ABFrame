using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class MonsterData : ExcelBase
{
    [XmlElement("MonsterDataList")]
    public List<MonsterDataBase> MonsterDataList { get; set; }

    [XmlIgnore]
    public Dictionary<int, MonsterDataBase> m_AllMonsterDic = new Dictionary<int, MonsterDataBase>();

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器下初始化类转xml
    /// </summary>
    public override void Construction()
    {
        // 测试数据，后续这里应该读表
        MonsterDataList = new List<MonsterDataBase>();
        for (int i = 0; i < 5; i++)
        {
            MonsterDataBase monster = new MonsterDataBase();
            monster.Id = i + 1;
            monster.Name = i + "sq";
            monster.OutLook = "Assets/GameResource/Prefabs/Door.prefab";
            monster.Rare = 2;
            monster.Height = i + 2;
            MonsterDataList.Add(monster);
        }
    }
#endif

    public override void Init()
    {
        m_AllMonsterDic.Clear();
        foreach (MonsterDataBase monster in MonsterDataList)
        {
            if (m_AllMonsterDic.ContainsKey(monster.Id))
            {
                Debug.LogError(monster.Name + " 有重复ID: " + monster.Id);
            }
            else
            {
                m_AllMonsterDic.Add(monster.Id, monster);
            }
        }
    }

    /// <summary>
    /// 根据id查找Monster数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MonsterDataBase FindMonsterById(int id)
    {
        return m_AllMonsterDic[id];
    }
}

[System.Serializable]
public class MonsterDataBase
{
    // Id
    [XmlAttribute("Id")]
    public int Id { get; set; }
    // Name
    [XmlAttribute("Name")]
    public string Name { get; set; }
    // 预知路径
    [XmlAttribute("OutLook")]
    public string OutLook { get; set; }
    // 怪物等级
    [XmlAttribute("Level")]
    public int Level { get; set; }
    // 怪物稀有度
    [XmlAttribute("Rare")]
    public int Rare { get; set; }
    // 怪物高度
    [XmlAttribute("Height")]
    public float Height { get; set; }
}
