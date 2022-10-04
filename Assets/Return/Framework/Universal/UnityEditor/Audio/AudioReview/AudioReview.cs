using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.IO;
using Object = UnityEngine.Object;

public class AudioReview : OdinEditorWindow
{
    public const string DataPath = "ReturnAudioAssets";
    public const string PrefFolderPath = "ReturnAudioAssets.FolderPath";
    public const string PrefTags = "ReturnAudioMarkTags";
    public const string PrefPreview = "ReturnAudioMarkNum";
    const string PrefLastKey= "AudioReview";

    [MenuItem("Windows/Return/Assets/AudioReview")]
    private static void OpenWindow()
    {
        var window = GetWindow<AudioReview>();

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 900);
    }

    [BoxGroup("Source",ShowLabel =false)]
    public MarksLibrary Library;

    [BoxGroup("Source", ShowLabel = false)]
    [HorizontalGroup("Source/Data")]
    [FolderPath][OnValueChanged(nameof(LoadFiles))]

    public string FolderPath;

    [Tooltip("Show current audio clip.")]
    [HorizontalGroup("Source/Data")]
    [Button("Ping")]
    public void Ping()
    {
        if (current == null)
            return;

        //UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(FolderPath, typeof(UnityEngine.Object));
        EditorGUIUtility.PingObject(current.GetClip);
        EditorUtility.OpenPropertyEditor(current.GetClip);
    }

    [BoxGroup("Source", ShowLabel = false)]
    [ShowInInspector]
    public int PreviewNumbers { get => EditorPrefs.GetInt(PrefPreview, 10); set => EditorPrefs.SetInt(PrefPreview, value); }


    /// <summary>
    /// Audio assets of current page
    /// </summary>
    [HideLabel]
    [BoxGroup("Files")]
    [EnumPaging]
    [OnValueChanged(nameof(ArchiveData),IncludeChildren =true,InvokeOnInitialize =false)]
    [TableList(AlwaysExpanded =true,DrawScrollView =true,ShowIndexLabels =true,MaxScrollViewHeight =80)]
    public AudioMark[] Marks;

    /// <summary>
    /// Guid
    /// </summary>
    string[] GUIDs;


    int _index;
    int NextIndex ()
    {
        _index = GUIDs.Loop(_index + 1);

        return _index;
    }
    int LastIndex()
    {
        _index = GUIDs.Loop(_index - 1);

        return _index;
    }


    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [PropertyOrder(1.1f)]
    [Button("ArchiveFile",ButtonSizes.Large)]
    public void ArchiveData()
    {
        Library.Archive(Marks);
    }

    /// <summary>
    /// GUID
    /// </summary>
    public string LastKey
    {
        get => EditorPrefs.GetString(PrefLastKey);
        set => EditorPrefs.SetString(PrefLastKey, value);
    }


    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("UpdateFiles", ButtonSizes.Large)]
    [PropertyOrder(1.5f)]
    public void LoadFiles()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            return;
        else
            EditorPrefs.SetString(PrefFolderPath, FolderPath);

        var folders= AssetDatabase.GetSubFolders(FolderPath);
        GUIDs = AssetDatabase.FindAssets("t:AudioClip", folders);
        var lastKey = LastKey;
        var length = GUIDs.Length;


        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("Loading", "Checking last record", (float)i / length);
            if (GUIDs[i].Equals(lastKey))
            {
                if (EditorUtility.DisplayDialog("Load", "Continue loading data with last record ?", "Yes, contiune last record. ", "No, create new search. "))
                    _index = i;
                else
                    _index = GUIDs.Length;

                break;
            }
        }


        EditorUtility.ClearProgressBar();

        NextPage();
    }


    bool VaildEdit => Marks != null;

    [EnableIf(nameof(VaildEdit))]
    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("NextPage",ButtonSizes.Large)]
    [PropertyOrder(2)]
    public void NextPage()
    {
        LoadPage(NextIndex);
    }

    void LoadPage(Func<int> index)
    {
        var key = index();

        var marks = new Queue<AudioMark>(PreviewNumbers);
        var first = key;


        for (int i = 0; i < PreviewNumbers; i++)
        {
            EditorUtility.DisplayProgressBar("Loading..", "Loading audio files", (float)i / PreviewNumbers);

            var guid = GUIDs[key];

            if (!Library.TryGetMark(guid, out var mark))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                mark = new AudioMark(clip);
            }

            marks.Enqueue(mark);
            key = index();

            if (key == first)
                break;
        }
        EditorUtility.ClearProgressBar();

        Marks = marks.ToArray();
        _select = Marks.Length;
        LastKey = GUIDs[first];
        PreviewNext();
    }
    [EnableIf(nameof(VaildEdit))]
    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("LastPage", ButtonSizes.Large)]
    [PropertyOrder(1)]
    public void LastPage()
    {
        LoadPage(LastIndex);
    }

    int _select;

    [EnableIf(nameof(VaildEdit))]
    [PropertySpace(10, SpaceAfter = 10)]
    [HorizontalGroup("Control")]
    [Button("PreviewNext", ButtonSizes.Large)]
    [PropertyOrder(4)]
    public void PreviewNext()
    {
        _select = Marks.Loop(_select+1);
        PreviewAudio();
    }

    [EnableIf(nameof(VaildEdit))]
    [PropertySpace(10,SpaceAfter =10)]
    [HorizontalGroup("Control")]
    [Button("PreviewLast", ButtonSizes.Large)]
    [PropertyOrder(3)]
    public void PreviewLast()
    {
        _select = Marks.Loop(_select-1);
        PreviewAudio();
    }

    void PreviewAudio()
    {
        current = Marks[_select];
        var clip = current.GetClip;
        AssetPreview.GetAssetPreview(clip);
        Selection.activeObject = clip;
        if (AssetDatabase.CanOpenAssetInEditor(clip.GetInstanceID()))
            AssetDatabase.OpenAsset(clip);
    }

    [PropertyOrder(20)]
    [BoxGroup][HideLabel]//[ShowInInspector]
    AudioMark current;

    Regex Regex;
    List<string> Tags;
    string newTag;
    IEnumerable<string> tags;
    Vector2 tagScroll;
    [SerializeField][HideInInspector]
    public Texture2D Star;
    [SerializeField]
    [HideInInspector]
    public Texture2D Star_off;



    [BoxGroup("Current Mark",ShowLabel =false)]
    [OnInspectorGUI]
    [PropertyOrder(21)]
    void ShowCurrent()
    {
        var score = current.Score;
        {
            if (current == null) return;
            Event e = Event.current;
            if (e.isKey)
            {
                var lastscore = score;
                switch (e.keyCode)
                {
                    case UnityEngine.KeyCode.Keypad9:
                        score = 9;
                        break;
                    case UnityEngine.KeyCode.Keypad8:
                        score = 8;
                        break;
                    case UnityEngine.KeyCode.Keypad7:
                        score = 7;
                        break;
                    case UnityEngine.KeyCode.Keypad6:
                        score = 6;
                        break;
                    case UnityEngine.KeyCode.Keypad5:
                        score = 5;
                        break;
                    case UnityEngine.KeyCode.Keypad4:
                        score = 4;
                        break;
                    case UnityEngine.KeyCode.Keypad3:
                        score = 3;
                        break;
                    case UnityEngine.KeyCode.Keypad2:
                        score = 2;
                        break;
                    case UnityEngine.KeyCode.Keypad1:
                        score = 1;
                        break;
                    case UnityEngine.KeyCode.Keypad0:
                        score = 10;
                        break;
                }
                if (lastscore != score)
                {
                    current.Score = score;
                }

            }
        }
        GUILayout.BeginHorizontal();
        var clip = current.GetClip;
        EditorGUILayout.LabelField("Name : " + clip.name);
        GUILayout.Label("Length : " + clip.length.ToString());
        GUILayout.Label("Frequency : " + clip.frequency.ToString());
        GUILayout.EndHorizontal();

        var layout = new[] { GUILayout.MinWidth(50), GUILayout.Height(50),GUILayout.MaxWidth(100) };
        
        var count = 11;
        GUILayout.BeginHorizontal();
        for (int i = 1; i < count; i++)
        {
            if (GUILayout.Button(i <= score ? Star:Star_off, layout))
                current.Score = i;
        }
        GUILayout.EndHorizontal();



        var style = GUI.skin.FindStyle("box");

        var m_tags = current.Tags.ToArray();
        var length = m_tags.Length;
        for (int i = 0; i < length; i++)
        {
            if (GUILayout.Button(m_tags[i]) &&Event.current.shift)
            {
                current.Tags.Remove(m_tags[i]);
                current.Tag = current.Tags.FirstOrDefault();
            }
        }

    }

    [BoxGroup("Add Tag", ShowLabel = false)]
    [OnInspectorGUI]
    [PropertyOrder(22)]
    protected void mGUI()
    {
        if (current == null)
            return;
        var windowWidth = EditorGUIUtility.currentViewWidth;

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space(50,true);


        var newtag = GUILayout.TextField(newTag,GUILayout.Width(120));

        if (GUILayout.Button("Add",GUILayout.Width(80)))
            if (!string.IsNullOrEmpty(newtag))
                if (!Tags.Contains(newtag))
                    Tags.Add(newtag);


        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        tagScroll =EditorGUILayout.BeginScrollView(tagScroll);
        var delete = string.Empty;

 
        EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth*0.65f));

        if (Regex!=null)
        {
            var keywords = Regex.Matches(current.GetClip.name);

            foreach (Match key in keywords)
            {
                var tag = key.Value;

                if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag))
                    continue;
                if (GUILayout.Button(tag))
                {
                    if (!current.Tags.Add(tag))
                    {
                        if (!Tags.Contains(tag))
                            Tags.Add(tag);
                        current.Tags.Remove(tag);
                        current.Tag = current.Tags.FirstOrDefault();
                    }
                    else
                        current.Tag = tag;
                }
            }
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth * 0.65f));

        var curWidth = windowWidth;
        foreach (var tag in tags)
        {
            if (GUILayout.Button(tag))
            {
                if (Event.current.shift)
                {
                    delete = tag;
                }
                else
                {
                    if (!current.Tags.Add(tag))
                    {
                        current.Tags.Remove(tag);
                        current.Tag = current.Tags.FirstOrDefault();
                    }
                    else
                        current.Tag = tag;
                }
            }
            var last = GUILayoutUtility.GetLastRect();
            var butWidth = last.width;
            curWidth -= butWidth;
            if (curWidth / windowWidth < 0.3f)
            {
                GUILayout.EndHorizontal();
                curWidth = windowWidth;
                GUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        //if (!string.IsNullOrEmpty(delete))
            Tags.Remove(delete);
        if (newTag != newtag)
        {
            OrderTags(newtag);
            newTag = newtag;
        }
    }

    void OrderTags(string key)
    {
        tags = Tags.OrderBy((x) => string.Compare(x, key));
    }

    private void Awake()
    {
        var list = GetAssetsAtPath<MarksLibrary>(MarksLibrary.DataPath).FirstOrDefault();
        if (!list)
        {
            list = ScriptableObject.CreateInstance<MarksLibrary>();
            var path = MarksLibrary.DataPath;

            var dir = Directory.Exists(path) ? null : Directory.CreateDirectory(path);

            path = Path.Combine(path, nameof(MarksLibrary));

            if (!path.Contains(".asset"))
                path += ".asset";

            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);


            AssetDatabase.CreateAsset(list, uniqueAssetPath);

            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(list);

            list= AssetDatabase.LoadAssetAtPath<MarksLibrary>(uniqueAssetPath);
        }
        Library = list;

        if(!Star)
        Star = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AudioMarks/Icon.png");
        if(!Star_off)
        Star_off = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AudioMarks/Icon_off.png");
        Regex = new Regex("[a-z|A-Z]*");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        FolderPath = EditorPrefs.GetString(PrefFolderPath,"Assets");
        Tags = new List<string>(EditorPrefs.GetString(PrefTags).Split('$'));
        tags = Tags;
        LoadFiles();

    }

    

    protected override void OnDestroy()
    {
        EditorPrefs.SetString(PrefTags,string.Join('$',Tags));
        ArchiveData();
        base.OnDestroy();

    }

    #region Baby
    public static T[] GetAssetsAtPath<T>(string path, bool inculdeChildFolder = false) where T : Object
    {

        EditorUtility.DisplayProgressBar("Loading", "Loading " + typeof(T) + " files..", 0);
        var fileEntries = GetSubFiles(path, true);
        EditorUtility.ClearProgressBar();

        if (fileEntries == null)
            return new T[0];

        ArrayList al = new ArrayList();

        foreach (string filePath in fileEntries)
        {
            Object t = AssetDatabase.LoadAssetAtPath<T>(filePath);
            if (t != null)
                al.Add(t);
        }
        T[] result = new T[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = (T)al[i];

        return result;
    }

    public static string[] GetSubFiles(string path, bool inculdeChildFolder)
    {
        var root = RootPath;
        path = path.Substring(path.IndexOf(root));

        if (path.Contains(root + "/"))
            path = path.Replace(root + "/", string.Empty);
        else
            path = path.Replace(root, string.Empty);


        path = Application.dataPath + "/" + path;

        if (!Directory.Exists(path))
            return null;

        var fileEntries = Directory.GetFiles(path).ToList();

        if (inculdeChildFolder)
        {
            var subFolders = Directory.GetDirectories(path);
            foreach (var folder in subFolders)
            {
                fileEntries.AddRange(GetSubFiles(folder, inculdeChildFolder));
            }
        }

        var length = fileEntries.Count;
        for (var i = 0; i < length; i++)
        {
            var filePath = fileEntries[i];
            int assetPathIndex = filePath.IndexOf(RootPath);
            fileEntries[i] = filePath.Substring(assetPathIndex);
        }

        return fileEntries.ToArray();
    }
    public const string RootPath = "Assets";
    #endregion
}

public static class m
{
    public static int Loop<T>(this T[] list, int sn)
    {
        var length = list.Length;
        if (length == 0)
            return -1;

        var remainder = sn % length;
        if (remainder < 0)
            return remainder + length;
        else
            return remainder;
    }
}