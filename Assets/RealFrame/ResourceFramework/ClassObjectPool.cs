using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T: class, new()
{
    private Stack<T> m_Pool = new Stack<T>();
    // 最大对象格式，<=0表示不限个数
    private int m_MaxCount = 0;
    // 没有回收的对象个数
    private int m_NoRecycleCount = 0;

    public ClassObjectPool(int maxCount)
    {
        m_MaxCount = maxCount;
        for (int i = 0; i < maxCount; i++)
        {
            m_Pool.Push(new T());
        }
    }

    public T Spawn(bool createIfPoolEmpty)
    {
        T rtn = null;
        if (m_Pool.Count > 0)
        {
            rtn = m_Pool.Pop();
        }

        if (rtn == null && createIfPoolEmpty)
        {
            rtn = new T();
        }

        if (rtn != null)
        {
            m_NoRecycleCount++;
        }

        return rtn;
    }

    public bool Recycle(T obj)
    {
        if (obj == null)
            return false;

        m_NoRecycleCount--;

        // 不放进对象池，由gc回收
        if (m_Pool.Count >= m_MaxCount && m_MaxCount >0)
        {
            obj = null;
            return false;
        }

        m_Pool.Push(obj);
        return true;
    }
}
