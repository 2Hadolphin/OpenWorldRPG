using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = MarksLibrary.DataPath, menuName ="CreateAudioMarks")]
[Serializable]
public class MarksLibrary : SerializedScriptableObject
{
    public const string DataPath = "Assets/AudioMarks";

    [HideLabel][SerializeField]
    protected HashSet<AudioMark> Marks;
    [SerializeField]
    protected AudioMark[] Data;


    public bool TryGetMark(string guid, out AudioMark mark)
    {
        return Marks.TryGetValue(AudioMark.Check(guid), out mark);
    }


    public void Archive(params AudioMark[] clips)
    {
        foreach (var clip in clips)
        {
            if (!Marks.Add(clip))
            {
                if (Marks.TryGetValue(clip, out var mark))
                    if (clip.Score.Equals(mark.Score))
                        break;
                    else
                    Debug.LogError(clip.Score + "\n" + mark.Score);
            }
        }

        EditorUtility.SetDirty(this);

        Debug.Log(string.Format("{0} has archive {1} data.", this.name, clips.Length));
    }

    [Button("Export as Json")]
    public void Export()
    {
        var setting = new JsonSerializerSettings() { Formatting = Formatting.Indented };


        var json = JsonConvert.SerializeObject(Marks.ToArray(), setting);
        var path = EditorUtility.SaveFilePanel("Save Audio Marks", string.Empty, "AudioMarks.json", "json");

        if (string.IsNullOrEmpty(path))
            return;

        var stream = new StreamWriter(path);

        stream.Write(json);
        stream.Close();
        stream.Dispose();

        Debug.Log(string.Format("{0} audio marks has been export.", Marks.Count));
    }

    [Button("Import from Json")]
    public void Import()
    {
        var path=EditorUtility.OpenFilePanel("Load Audio Marks", string.Empty, "json");
        if (!File.Exists(path))
            return;

        var reader = new StreamReader(path);

        var json = reader.ReadToEnd();
        reader.Close();
        reader.Dispose();

        var array = JsonConvert.DeserializeObject<AudioMark[]>(json);

        Archive(array);

        Debug.Log(string.Format("{0} audio marks has been import.",array.Length));
    }


    private void OnEnable()
    {
        if (Marks == null)
            Marks = new HashSet<AudioMark>(100);
    }

    protected override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();
        if (Marks != null)
        {
            Data = Marks.ToArray();
            //          Marks.Clear();
        }
    }

    protected override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
        if (Data != null)
        {
            Marks = new HashSet<AudioMark>(Data);
            Data = null;
        }

    }

}

[Serializable]
[HideLabel]
public class AudioMark : ISerializationCallbackReceiver, IEquatable<AudioMark>, IEquatable<AudioClip>, IEquatable<string>
{
    static AudioMark Compare = new AudioMark();
    public static AudioMark Check(string guid)
    {
        Compare.GUID = guid;
        return Compare;
    }

    private AudioMark() 
    {
        if(!string.IsNullOrEmpty(FilePath))
        Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(FilePath);
    }
    public AudioMark(AudioClip clip)
    {
        Clip = clip;
        FilePath = AssetDatabase.GetAssetPath(clip);
        GUID = AssetDatabase.AssetPathToGUID(FilePath);
        Tags = new HashSet<string>();
    }

    [TableColumnWidth(15)]
    [ShowInInspector]
    [ReadOnly]
    [JsonIgnore]
    [HideLabel]
    protected AudioClip Clip;

    [JsonIgnore]
    protected string GUID;

    [HideInInspector]
    public string FilePath;


    #region Properties
    [TableColumnWidth(30)]
    [Range(0,10)]
    public int Score=0;

    [TableColumnWidth(70)]
    [JsonProperty]
    public HashSet<string> Tags;

    [ValueDropdown(nameof(Tags))]
    [JsonIgnore]
    [NonSerialized]
    [ShowInInspector]
    public string Tag;

    #endregion

    public void OnBeforeSerialize()
    {
        if (!string.IsNullOrEmpty(FilePath))
            FilePath = AssetDatabase.GetAssetPath(Clip);
    }

    public void OnAfterDeserialize()
    {
        //        Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(FilePath);
        if (Tags == null)
            Tags = new HashSet<string>();
    }

    

    public bool IsValid()
    {
        return Clip;
    }
    [JsonIgnore]
    public AudioClip GetClip => Clip;
    public bool Equals(AudioMark other)
    {
        return Clip == other.Clip;
    }

    public bool Equals(AudioClip other)
    {
        if (Clip)
            return Clip == other;
        else
            return FilePath == AssetDatabase.GetAssetPath(other);
    }

    public bool Equals(string other)
    {
        return GUID== other;
    }

    public static implicit operator string (AudioMark mark)
    {
        return mark.GUID;
    }
}