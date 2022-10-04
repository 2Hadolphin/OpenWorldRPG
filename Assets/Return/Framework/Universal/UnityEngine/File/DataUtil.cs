using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class DataUtil
{
    public static string GetPath(object reference)
    {
        return reference.GetType().ToString();
    }


    public static void SaveOriginFile(object Reference, object Data)
    {
        string Path = "\\"+GetPath(Reference)+".dh";
        SaveFile(Path, Data);
    }

    public static void SaveFile(string Path,object Data)
    {
        FileStream fs = new FileStream(Application.persistentDataPath + Path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, Data);
        fs.Close();
        Debug.Log(Data + " has been save to " + Application.persistentDataPath + Path);
    }

    public static bool LoadFile(Type type, out object data)
    {
        string Path = "\\" + type.ToString() + ".dh";
        try
        {
            if (System.IO.File.Exists(Application.persistentDataPath + Path))
            {

                FileStream fs = System.IO.File.Open(Application.persistentDataPath + Path, FileMode.Open);
                fs.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                data = bf.Deserialize(fs);
                fs.Close();

                if(data.GetType().Equals(type))
                {
                    Debug.Log(data.GetType() + " has been extract from " + Application.persistentDataPath + Path);
                    return true;
                }
                else
                {
                    Debug.Log(data.GetType() + "'s Type doesn't match the as " + type );
                    return false;
                }
     
            }
            else
            {

                data = null;
                return false;
            }
        }
        catch (Exception)
        {
            data = null;
            return false;
            throw;
        }

    }



}
