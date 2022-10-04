using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Return.Database.Assets;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
using System.Text.RegularExpressions;
using Return;
using Return.Database;

public class SpritePresetToolkit : OdinEditorWindow
{
    [MenuItem("Tools/Preset/SpriteToolkit")]
    private static void OpenWindow()
    {
        GetWindow<SpritePresetToolkit>().position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
    }

    [SerializeField]
    public SpritePresetBundle PresetBundle;

    bool HasBundle => null != PresetBundle;



    [EnableIf(nameof(HasBundle))]
    protected override void OnGUI()
    {
        base.OnGUI();


    }

    public bool CustomPresetPath;

    [ShowIf(nameof(CustomPresetPath))]
    
    public FolderPathFinder Path;

    [BoxGroup("Preset State")]
    public AssetsSourceType SourceType;



    [BoxGroup("Preset State")]
    [ListDrawerSettings(Expanded = true)]
    public List<string> OverrideTags = new();

    [BoxGroup("Preset State")]
    public bool ParseTagViaPath;

    [BoxGroup("Preset State")]
    [ShowIf(nameof(ParseTagViaPath))]
    public string RegexPattern;

    [ListDrawerSettings(Expanded =true)]
    public List<UnityEngine.Object> Targets=new ();


    [Button(nameof(ParseSprite2Preset))]
    void ParseSprite2Preset()
    {
        foreach (var target in Targets)
        {
            Sprite newSpr = null;

            if (AssetDatabase.IsForeignAsset(target))
            {
                if (target is Sprite sprite)
                    newSpr = sprite;
                else
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
                    BuildSpritePreset(subAssets.Where(x => x is Sprite).Select(x => x as Sprite).ToArray());
                    return;
                }
            }
            else if(target is GameObject obj)
            {
                var components = obj.GetComponentsInChildren<SpriteRenderer>();
                BuildSpritePreset(components.Select(x => x.sprite).ToArray());
                return;
            }
            else if(target is SpriteRenderer ren)
            {
                newSpr = ren.sprite;
            }

            if(newSpr)
            {
                // create so and save sprite reference
                BuildSpritePreset(newSpr);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sprites">must be an asset inside project</param>
    [Button(nameof(BuildSpritePreset))]
    public virtual void BuildSpritePreset(params Sprite[] sprites)
    {
        var prestTitle = nameof(SpritePreset);

        Dictionary<string, List<string>> parseTag =
            ParseTagViaPath?
            new (sprites.Length):
            null;


        Regex regex =
            ParseTagViaPath&& !string.IsNullOrEmpty(RegexPattern) ?
            new Regex(RegexPattern) :
            null;
        

        foreach (var spr in sprites)
        {
            var preset = CreateInstance<SpritePreset>();

            var sprID = spr.name;

            preset.Title = sprID;
            preset.Asset = spr;
            preset.LoadGUID();
            preset.Tags.AddRange(OverrideTags);

            var sprPath = AssetDatabase.GetAssetPath(spr);

            if (ParseTagViaPath && regex != null)
            {
                if(!parseTag.TryGetValue(sprPath,out var catchTags))
                {
                    var folderName = EditorPathUtility.GetAssetFolderName(sprPath);
                    catchTags = regex.Matches(folderName).Select(x => x.Value).ToList();
                    parseTag.Add(sprPath, catchTags);
                }

                preset.Tags.AddRange(catchTags);
            }

            var path = CustomPresetPath ?
                Path :
                sprPath;

            EditorHelper.WriteAsset(preset,string.Format("{0}_{1}", prestTitle, sprID), path);
        }
    }

}
