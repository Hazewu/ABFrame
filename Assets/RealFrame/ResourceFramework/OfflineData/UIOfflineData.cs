using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIOfflineData : OfflineData
{
    public Vector2[] m_AllAnchorMax;
    public Vector2[] m_AllAnchorMin;
    public Vector2[] m_AllPivot;
    public Vector2[] m_AllSizeDelta;
    public Vector3[] m_AllAnchoredPos;
    public ParticleSystem[] m_AllParticle;

    public override void ResetProp()
    {
        int pointCount = m_AllPoint.Length;
        for (int i = 0; i < pointCount; i++)
        {
            RectTransform temp = m_AllPoint[i] as RectTransform;
            if (temp != null)
            {
                temp.localPosition = m_AllPos[i];
                temp.localScale = m_AllScale[i];
                temp.localRotation = m_AllRot[i];

                temp.pivot = m_AllPivot[i];
                temp.anchorMax = m_AllAnchorMax[i];
                temp.anchorMin = m_AllAnchorMin[i];
                temp.sizeDelta = m_AllSizeDelta[i];
                temp.anchoredPosition3D = m_AllAnchoredPos[i];

                // 设置激活态（状态不同才设置）
                bool needSetActive = m_AllActive[i] != temp.gameObject.activeSelf;
                if (needSetActive)
                {
                    temp.gameObject.SetActive(m_AllActive[i]);
                }


                // 多的可以删，少的补不了，最好不要动结构
                if (temp.childCount > m_AllChildCount[i])
                {
                    Debug.LogWarning("注意该物体结构发生变化:" + temp.gameObject.name);
                    int childCount = temp.childCount;
                    for (int j = m_AllChildCount[i]; j < childCount; j++)
                    {
                        GameObject tempObj = temp.GetChild(j).gameObject;
                        // 删掉非对象池中创建出来的物体
                        if (!ObjectManager.Instance.IsObjectManagerCreate(tempObj))
                        {
                            GameObject.Destroy(tempObj);
                        }
                    }
                }
            }
        }

        int particleCount = m_AllParticle.Length;
        for (int i = 0; i < particleCount; i++)
        {
            m_AllParticle[i].Clear(true);
            m_AllParticle[i].Play();
        }
    }

    public override void BindData()
    {
        Transform[] allTrans = GetComponentsInChildren<Transform>(true);
        int transCount = allTrans.Length;
        for (int i = 0; transCount > 0; transCount--)
        {
            if (!(allTrans[i] is RectTransform))
            {
                allTrans[i].gameObject.AddComponent<RectTransform>();
            }
        }

        m_AllPoint = GetComponentsInChildren<RectTransform>(true);
        m_AllParticle = GetComponentsInChildren<ParticleSystem>(true);
        int pointCount = m_AllPoint.Length;
        m_AllChildCount = new int[pointCount];
        m_AllActive = new bool[pointCount];
        m_AllPos = new Vector3[pointCount];
        m_AllScale = new Vector3[pointCount];
        m_AllRot = new Quaternion[pointCount];

        m_AllPivot = new Vector2[pointCount];
        m_AllAnchorMax = new Vector2[pointCount];
        m_AllAnchorMin = new Vector2[pointCount];
        m_AllSizeDelta = new Vector2[pointCount];
        m_AllAnchoredPos = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            RectTransform temp = m_AllPoint[i] as RectTransform;
            m_AllChildCount[i] = temp.childCount;
            m_AllActive[i] = temp.gameObject.activeSelf;
            m_AllPos[i] = temp.localPosition;
            m_AllScale[i] = temp.localScale;
            m_AllRot[i] = temp.localRotation;

            m_AllPivot[i] = temp.pivot;
            m_AllAnchorMax[i] = temp.anchorMax;
            m_AllAnchorMin[i] = temp.anchorMin;
            m_AllSizeDelta[i] = temp.sizeDelta;
            m_AllAnchoredPos[i] = temp.anchoredPosition3D;
        }
    }
}
