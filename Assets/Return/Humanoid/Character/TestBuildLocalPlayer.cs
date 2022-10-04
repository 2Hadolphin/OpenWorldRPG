using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Character;
using Sirenix.OdinInspector;
using Return.Agents;
using Return;
using Return.Humanoid;

public class TestBuildLocalPlayer : MonoBehaviour
{
    
    public CharacterPreset Preset;


    [Button("BuildLocalPlayer")]
    public void BuildPlayer()
    {
        var cha = Preset.BuildCharacter();
        var agent=cha.InstanceIfNull<HumanoidAgent>();

    }

    [OnValueChanged(nameof(AddSystem))]
    public UnityEngine.Object Target;

    void AddSystem()
    {
        if(Target)
        {
            var wrapper = new ClassWrapper(Target);

            var result = Systems.Add(wrapper);

            Debug.Log(string.Format("Add {0} {1}", wrapper, result ? "successfully" : "failure"));
        }
        else
        {
            
        }

    }

    [ShowInInspector, ReadOnly]
    protected HashSet<ClassWrapper> Systems = new HashSet<ClassWrapper>();

    [Button(nameof(GetSystem))]
    void GetSystem()
    {
        var result = Systems.TryGetValue(new ClassWrapperInjector(Target.GetType()),out var wrapper);
        if (result)
            Debug.Log(wrapper.System);
        else
            Debug.Log(result);
    }
    [ShowInInspector]
    UnityEngine.Object[] objs;
    [Button("find")]
    void FindOBJ()
    {
        //objs = FindObjectsOfType<MotionModule>();

    }

    [Button("Delete")]
    void Delete()
    {
        var length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            DestroyImmediate(objs[i]);
        }

    }

}
