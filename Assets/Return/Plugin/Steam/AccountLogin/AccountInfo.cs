using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccountInfo
{
    public string UID;
    public string ID;
    public string Account;
    public string Password;

    public bool VerifyAccount(string account)
    {
        return account == Account;
    }


    public bool VerifyPassword(string password)
    {
        return password == Password;
    }

    public string GetUserData(string password)
    {
        if (password == Password)
        {
            return UID;
        }
        else
        {
            return "Error Verify";
        }
    }

}
