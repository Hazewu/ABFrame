using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TestGameStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestLoadAB();
    }

    private void TestLoadAB()
    {
        string path = Application.streamingAssetsPath + "/assetbundleconfig";
        AssetBundle configAB = AssetBundle.LoadFromFile(path);
        Debug.Log("path:" + path);
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig testSerilize = (AssetBundleConfig)bf.Deserialize(stream);
        stream.Close();

        string prefabPath = "Assets/GameResources/Prefabs/Door.prefab";
        uint crc = CRC32.GetCRC32(prefabPath);
        ABBase abBase = null;
        for (int i = 0; i < testSerilize.ABList.Count; i++)
        {
            if (testSerilize.ABList[i].Crc == crc)
            {
                abBase = testSerilize.ABList[i];
                break;
            }
        }

        for (int i = 0; i < abBase.ABDepends.Count; i++)
        {
            // 先加载依赖
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDepends[i]);
        }
        AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
        GameObject obj = Instantiate(ab.LoadAsset<GameObject>(abBase.AssetName));
    }
}
