using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T m_instance;

    public static T Instance
    {
        get { return m_instance; }
    }

    protected virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = (T)this;
        }
        else
        {
            Debug.LogError("Get a second instance of this class" + this.GetType());
        }
    }
}
