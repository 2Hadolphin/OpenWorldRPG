using System;

public class ValueConfig : SettingConfig
{

}


public class ValueConfig<T> : ValueConfig
{
    protected T m_Value;
    public T Value
    {
        get => NewValue;
        set
        {
            m_Value = value;
            NewValue = value;
        }
    }
    public Action<T> Callback;
    public bool Dirty { get; protected set; } = false;
    public T NewValue { get; protected set; }

    public virtual void SetValue(T value)
    {
        Dirty = true;
        NewValue = value;
    }

    public virtual void ApplyModify()
    {
        if (Dirty)
        {
            if (!m_Value.Equals(NewValue))
            {
                Callback.Invoke(NewValue);
                m_Value = NewValue;
            }

            Dirty = false;
        }
    }
}
