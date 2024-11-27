using System;
using UnityEngine;

public class TaskAction
{
    private Action m_Callback;
    private int m_Current;
    private int m_Count;
    public int GetCurrent => m_Current;
    public int GetCount => m_Count;
    private string m_loggingName = "";

    public bool IsComplete()
    {
        return (m_Count == m_Current);
    }

    public TaskAction(int count, Action callback, string loggingName = "")
    {
        m_loggingName = loggingName;
        if (count == 0)
        {
            callback?.Invoke();
        }
        else
        {
            m_Callback = callback;
            m_Count = count;
        }
    }

    public void Increment()
    {
        m_Current++;

        if (false == string.IsNullOrEmpty(m_loggingName))
        {
            Debug.Log($"{m_loggingName}, {m_Current}/{m_Count}");
        }
        if (m_Current == m_Count)
        {
            m_Callback?.Invoke();
        }
    }
}
