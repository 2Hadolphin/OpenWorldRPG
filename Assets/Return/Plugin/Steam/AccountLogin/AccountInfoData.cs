using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Data_Account")]
public class AccountInfoData : ScriptableObject
{
    public AccountInfo[] Accounts;

    public Storage SaveData()
    {
        return new Storage(Accounts);
    }
    public void LoadData(Storage storage)
    {
        Accounts = storage.Accounts;
    }

    [Serializable]
    public class Storage
    {
        public Storage(AccountInfo[] accounts)
        {
            Accounts = accounts;
        }
        public readonly AccountInfo[] Accounts;
    }
}
