using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineData : MonoBehaviour
{
    public Rigidbody m_RigidBody;
    public Collider m_Collider;
    public Transform[] m_AllPoint;
    public int[] m_AllChildCount;
    public bool[] m_AllActive;
    public Vector3[] m_AllPos;
    public Vector3[] m_AllScale;
    public Quaternion[] m_AllRot;

    /// <summary>
    /// ��ԭ����
    /// </summary>
    public virtual void ResetProp()
    {
        int pointCount = m_AllPoint.Length;
        for (int i = 0; i < pointCount; i++)
        {
            Transform temp = m_AllPoint[i];
            if (temp != null)
            {
                temp.localPosition = m_AllPos[i];
                temp.localScale = m_AllScale[i];
                temp.localRotation = m_AllRot[i];

                // ���ü���̬��״̬��ͬ�����ã�
                bool needSetActive = m_AllActive[i] != temp.gameObject.activeSelf;
                if (needSetActive)
                {
                    temp.gameObject.SetActive(m_AllActive[i]);
                }


                // ��Ŀ���ɾ���ٵĲ����ˣ���ò�Ҫ���ṹ
                if (temp.childCount > m_AllChildCount[i])
                {
                    Debug.LogWarning("ע�������ṹ�����仯:" + temp.gameObject.name);
                    int childCount = temp.childCount;
                    for (int j = m_AllChildCount[i]; j < childCount; j++)
                    {
                        GameObject tempObj = temp.GetChild(j).gameObject;
                        // ɾ���Ƕ�����д�������������
                        if (!ObjectManager.Instance.IsObjectManagerCreate(tempObj))
                        {
                            GameObject.Destroy(tempObj);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// �༭��ģʽ�±����ʼ����
    /// </summary>
    public virtual void BindData()
    {
        m_Collider = GetComponentInChildren<Collider>(true);
        m_RigidBody = GetComponentInChildren<Rigidbody>(true);

        // �����ӽڵ�
        m_AllPoint = GetComponentsInChildren<Transform>(true);
        int pointCount = m_AllPoint.Length;
        m_AllChildCount = new int[pointCount];
        m_AllActive = new bool[pointCount];
        m_AllPos = new Vector3[pointCount];
        m_AllScale = new Vector3[pointCount];
        m_AllRot = new Quaternion[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Transform temp = m_AllPoint[i];
            m_AllChildCount[i] = temp.childCount;
            m_AllActive[i] = temp.gameObject.activeSelf;
            m_AllPos[i] = temp.localPosition;
            m_AllScale[i] = temp.localScale;
            m_AllRot[i] = temp.localRotation;
        }
    }
}
