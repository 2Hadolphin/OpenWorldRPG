using System;
using UnityEngine;
using Return;

[Obsolete]
public class AccountManager : MonoBehaviour
{
    private static AccountInfoData Data;
    public void Init()
    {
        if (!Data)
        {
            if (DataUtil.LoadFile(typeof(AccountInfoData.Storage), out var data))
            {
                Data = ScriptableObject.CreateInstance<AccountInfoData>();
                Data.LoadData(data as AccountInfoData.Storage);
            }
            else
            {
                Data = new AccountInfoData();
            }
        }
    }

    public bool Verify(string[] input)
    {


        int length = Data.Accounts.Length;
        for (int i = 0; i < length; i++)
        {
            if (input[0].Equals(Data.Accounts[i].Account))
            {
                Debug.Log(".... Account Correct !");
                if (input[1].Equals(Data.Accounts[i].Password))
                {
                    Debug.Log(".... Password Correct !");
                    confirmLogin(Data.Accounts[i].GetUserData(input[1]));
                    return true;
                }
            }
        }
        return false;
    } 

    private void confirmLogin(string UID)
    {
        //SCMA.AssignUersData(UID);
    }

    
}

