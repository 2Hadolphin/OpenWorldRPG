
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

public static class IOExtension
{
    public const string Json = ".txt";
    public const string UnityObject = ".asset";

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="card"></param>
    /// <param name="path">file path</param>
    public static void Save<T>(T card,string path) where T: class
    {
        BinaryFormatter formatter = new BinaryFormatter();
        path = Application.persistentDataPath +"/"+ path+ UnityObject;

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, card);
        stream.Close();

    }

    //Loading Data
    public static T Load<T>(string path) where T : class
    {
        path = Path.Combine(Application.persistentDataPath , path , UnityObject);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            //Convert from Binary to Strings
            T data = formatter.Deserialize(stream) as T;
            stream.Close();
            //Final Return
            return data;
        }
        else
        {
            Debug.LogError("Saved so not found in " + path);
            return null;
        }
    }


    private static void WriteToFile(string fileName, string json)
    {
        var path = GetFilePath(fileName);

        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(json);
            }
        }
    }

    public static void DeleteCharacter(string characterName)
    {
        var filePath = Path.Combine(Application.persistentDataPath , characterName , "_CharacterData.txt");

        // check if file exists
        if (!File.Exists(filePath))
            Debug.LogError("This character save file does not exist");
        else
            File.Delete(filePath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"> path doesn't exist root directory</param>
    private static string ReadFromFile(string fileName)
    {
        var path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            using (var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                return json;
            }
        }
        else
        {
            return "";
        }
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath , fileName);
    }

    public static void SaveData<T>(string path,string fileName, T data)
    {
        var info=System.IO.Directory.CreateDirectory(GetFilePath(path));

        var json = JsonUtility.ToJson(data);
        WriteToFile(Path.Combine(path,fileName+Json), json);
    }

    public static T LoadData<T>(string fileName) where T:new ()
    {
        T curData = new T();
        var json = ReadFromFile(fileName + Json);
        JsonUtility.FromJsonOverwrite(json, curData);
        
        return curData;
    }

    public static T LoadSO<T>(string fileName) where T : ScriptableObject
    {

        T curData = ScriptableObject.CreateInstance<T>();
        var json = ReadFromFile(fileName + Json);
        JsonUtility.FromJsonOverwrite(json, curData);
        return curData;
    }

    public static T LoadArchive<T>(string directory,string fileName=null)where T:new ()
    {
        if (!string.IsNullOrEmpty(fileName))
            directory = Path.Combine(directory , fileName);
        else
            Debug.Log(fileName);

        var path = GetFilePath(directory)+Json;

        if (!File.Exists(path))
        {
            throw new DirectoryNotFoundException(path);
        }

        var a = new T();

        var json = ReadFromFile(path);
        JsonUtility.FromJsonOverwrite(json,a);
        return a;

        //var value = Archive<T>.Create(null);
        //var json = ReadFromFile(path);
        //JsonUtility.FromJsonOverwrite(json,value);

        //return value;
    }

    public static string LoadArchive(string directory, string fileName = null) 
    {
        if (!string.IsNullOrEmpty(fileName))
            directory = Path.Combine(directory ,fileName);
        else
            Debug.Log(fileName);

        var path = GetFilePath(directory) + Json;

        if (!File.Exists(path))
        {
            throw new DirectoryNotFoundException(path);
        }
        

        var json = ReadFromFile(path);

        string str;

        try
        {
            str= JsonUtility.FromJson<string>(json);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }

        return str;

        //var value = Archive<T>.Create(null);
        //var json = ReadFromFile(path);
        //JsonUtility.FromJsonOverwrite(json,value);

        //return value;
    }

    public static void SaveJson<T>(string path, string fileName, T data)where T:new ()
    {
        var info = System.IO.Directory.CreateDirectory(GetFilePath(path));
        var settings = new Newtonsoft.Json.JsonSerializerSettings();

        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.Formatting = Formatting.Indented;

        var json = JsonConvert.SerializeObject(data,settings);
        WriteToFile(Path.Combine(path, fileName + Json), json);
    }

    public static T LoadJson<T>(string directory, string fileName = null) where T : new()
    {
        if (!string.IsNullOrEmpty(fileName))
            directory = Path.Combine(directory, fileName);

        var path = directory.Contains(Json) ? directory : directory + Json;

        var json = ReadFromFile(path);

        var obj = JsonConvert.DeserializeObject<T>(json);
        return obj;
    }


    //public static T LoadJson<T>(string directory, string fileName) where T:new ()
    //{
    //    if (!string.IsNullOrEmpty(fileName))
    //        directory = Path.Combine(directory,fileName);
    //    else
    //        Debug.Log(fileName);

    //    var path = directory + Json;

    //    var json = ReadFromFile(path);
        
    //    var obj = JsonConvert.DeserializeObject<T>(json);
    //    return obj;
    //}

    /// <summary>
    /// 
    /// </summary>
    public static T LoadJsonListSO<T>(string directory, string fileName) where T:ScriptableObject
    {
        if (!string.IsNullOrEmpty(fileName))
            directory = Path.Combine(directory, fileName);


        var path = directory + Json;

        var json = ReadFromFile(path);
        var list = ScriptableObject.CreateInstance<T>();

        JsonUtility.FromJsonOverwrite(json,list);
        return list;
    }


    public static void Save(object gameState, string fileName = "gamesave.dat")
    {
        var serializedData = JsonConvert.SerializeObject(gameState);
        byte[] bytes = System.Text.Encoding.Default.GetBytes(serializedData);
        var filePath =  Path.Combine(Application.persistentDataPath , fileName);
        File.WriteAllBytes(filePath, bytes);
    }

    public static T Loadd<T>(string fileName = "gamesave.dat")
    {
        var filePath =Path.Combine( Application.persistentDataPath, fileName);

        try
        {            
            var serializedData = File.ReadAllBytes(filePath);
            string json = System.Text.Encoding.Default.GetString(serializedData);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.IO.FileNotFoundException)
        {
            return default;
        }
 
    }


    ///// <summary>
    ///// 
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="card"></param>
    ///// <param name="path">file path</param>
    //public static void Save<T>(T card, string path) where T : class
    //{
    //    BinaryFormatter formatter = new BinaryFormatter();
    //    path = Application.persistentDataPath + "/" + path + UnityObject;

    //    FileStream stream = new FileStream(path, FileMode.Create);

    //    formatter.Serialize(stream, card);
    //    stream.Close();

    //}

    ////Loading Data
    //public static T Load<T>(string path) where T : class
    //{
    //    path = Path.Combine(Application.persistentDataPath, path, UnityObject);

    //    if (File.Exists(path))
    //    {
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        FileStream stream = new FileStream(path, FileMode.Open);
    //        //Convert from Binary to Strings
    //        T data = formatter.Deserialize(stream) as T;
    //        stream.Close();
    //        //Final Return
    //        return data;
    //    }
    //    else
    //    {
    //        Debug.LogError("Saved so not found in " + path);
    //        return null;
    //    }
    //}


    private static void WriteToFile(string targetPath, string fileName, string json)
    {
        var path = string.IsNullOrEmpty(targetPath) ? GetFilePath(fileName) : GetFilePath(fileName, targetPath);
        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(json);
            }
        }
    }

    //public static void DeleteCharacter(string characterName)
    //{
    //    var filePath = Path.Combine(Application.persistentDataPath, characterName, "_CharacterData.txt");

    //    // check if file exists
    //    if (!File.Exists(filePath))
    //        Debug.LogError("This character save file does not exist");
    //    else
    //        File.Delete(filePath);
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="fileName"> path doesn't exist root directory</param>
    //private static string ReadFromFile(string fileName)
    //{
    //    var path = GetFilePath(fileName);
    //    if (File.Exists(path))
    //    {
    //        using (var reader = new StreamReader(path))
    //        {
    //            var json = reader.ReadToEnd();
    //            return json;
    //        }
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //}


    private static string GetFilePath(string fileName, string path = null)
    {
        if (string.IsNullOrEmpty(path))
            return Path.Combine(Application.persistentDataPath, fileName);
        else
            return Path.Combine(path, fileName);
    }

    //public static void SaveData<T>(string path, string fileName, T data)
    //{
    //    var info = System.IO.Directory.CreateDirectory(GetFilePath(path));

    //    var json = JsonUtility.ToJson(data);
    //    WriteToFile(path, fileName + Json, json);
    //}

    //public static T LoadData<T>(string fileName) where T : new()
    //{
    //    T curData = new T();
    //    var json = ReadFromFile(fileName + Json);
    //    JsonUtility.FromJsonOverwrite(json, curData);

    //    return curData;
    //}

    //public static T LoadSO<T>(string fileName) where T : ScriptableObject
    //{

    //    T curData = ScriptableObject.CreateInstance<T>();
    //    var json = ReadFromFile(fileName + Json);
    //    JsonUtility.FromJsonOverwrite(json, curData);
    //    return curData;
    //}

    //public static T LoadArchive<T>(string directory, string fileName = null) where T : new()
    //{
    //    if (!string.IsNullOrEmpty(fileName))
    //        directory = Path.Combine(directory, fileName);
    //    else
    //        Debug.Log(fileName);

    //    var path = GetFilePath(directory) + Json;

    //    if (!File.Exists(path))
    //    {
    //        throw new DirectoryNotFoundException(path);
    //    }

    //    var a = new T();

    //    var json = ReadFromFile(path);
    //    JsonUtility.FromJsonOverwrite(json, a);
    //    return a;

    //    //var value = Archive<T>.Create(null);
    //    //var json = ReadFromFile(path);
    //    //JsonUtility.FromJsonOverwrite(json,value);

    //    //return value;
    //}

    //public static string LoadArchive(string directory, string fileName = null)
    //{
    //    if (!string.IsNullOrEmpty(fileName))
    //        directory = Path.Combine(directory, fileName);
    //    else
    //        Debug.Log(fileName);

    //    var path = GetFilePath(directory) + Json;

    //    if (!File.Exists(path))
    //    {
    //        throw new DirectoryNotFoundException(path);
    //    }


    //    var json = ReadFromFile(path);
    //    var a = JsonUtility.FromJson<string>(json);
    //    return a;

    //    //var value = Archive<T>.Create(null);
    //    //var json = ReadFromFile(path);
    //    //JsonUtility.FromJsonOverwrite(json,value);

    //    //return value;
    //}



    //public static void SaveJson<T>(string path, string fileName, T data) where T : new()
    //{
    //    var info = System.IO.Directory.CreateDirectory(string.IsNullOrEmpty(fileName) ? GetFilePath(path) : path);
    //    var settings = new Newtonsoft.Json.JsonSerializerSettings();

    //    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    //    settings.Formatting = Formatting.Indented;

    //    var json = JsonConvert.SerializeObject(data, settings);

    //    WriteToFile(path, fileName + Json, json);
    //}


    ///// <summary>
    ///// 
    ///// </summary>
    //public static T LoadJsonListSO<T>(string directory, string fileName) where T : ScriptableObject
    //{
    //    if (!string.IsNullOrEmpty(fileName))
    //        directory = Path.Combine(directory, fileName);


    //    var path = directory + Json;

    //    var json = ReadFromFile(path);
    //    var list = ScriptableObject.CreateInstance<T>();

    //    JsonUtility.FromJsonOverwrite(json, list);
    //    return list;
    //}


    //public static void Save(object gameState, string fileName = "gamesave.dat")
    //{
    //    var serializedData = JsonConvert.SerializeObject(gameState);
    //    byte[] bytes = System.Text.Encoding.Default.GetBytes(serializedData);
    //    var filePath = Path.Combine(Application.persistentDataPath, fileName);
    //    File.WriteAllBytes(filePath, bytes);
    //}

    //public static T Loadd<T>(string fileName = "gamesave.dat")
    //{
    //    var filePath = Path.Combine(Application.persistentDataPath, fileName);

    //    try
    //    {
    //        var serializedData = File.ReadAllBytes(filePath);
    //        string json = System.Text.Encoding.Default.GetString(serializedData);
    //        return JsonConvert.DeserializeObject<T>(json);
    //    }
    //    catch (System.IO.FileNotFoundException)
    //    {
    //        return default;
    //    }

    //}
}