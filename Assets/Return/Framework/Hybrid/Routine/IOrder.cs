using System;

namespace Return
{
    public interface IOrder: IComparable<IOrder>
    {
        int IComparable<IOrder>.CompareTo(IOrder other)
        {
            return  other.GetExecuteOrder() - GetExecuteOrder();
        }

        int GetExecuteOrder();
    }
}