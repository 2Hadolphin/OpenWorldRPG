using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return;
using Newtonsoft.Json;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.Assertions;

namespace Return.Editors
{
    public class MotionStateEditor
    {
        public MotionStateEditor()
        {
            StatesColleciotn = new Dictionary<string, VirtualValue>();
        }

        #region Storage Path
        public const string DataPath = "MotionStateEditor";
        [PropertyOrder(-5)]
        [FolderPath]
        [ShowInInspector]
        public string Path
        {
            get => EditorPrefs.GetString(DataPath, "Assets/Return/Resources/");
            set => EditorPrefs.SetString(DataPath, value);
        }
        #endregion

        #region States
        const string States = "StateGroup";
        [BoxGroup(States)]
        [TextArea(3, 5)]
        string m_Description;
        [BoxGroup(States)]
        [Button("CreateState")]
        public void CreateState(string id, VirtualValue type)
        {
            var title = type.ToString() + "_" + id;
            var state = EditorAssetsUtility.CreateInstanceSO<MotionState>(false, Path, title + ".asset");
            state.Title = title;
            state.EventID = id;
            state.SetValueType = type;
            state.Description = m_Description;
            state.Dirty();
        }

        [BoxGroup(States)]
        [AssetList]
        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 5)]
        public List<MotionState> PresetStates = new List<MotionState>();

        [BoxGroup(States)]
        [ListDrawerSettings(Expanded = true)]
        List<MotionState> Errors = new List<MotionState>();

        [BoxGroup(States)]
        [ShowInInspector]
        [HideLabel]
        [Searchable]
        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 5)]
        Dictionary<string, VirtualValue> StatesColleciotn;

        [BoxGroup(States)]
        [Button("Parse Selected Into Dictionary")]
        void LoadAll()
        {
            var str = EditorPrefs.GetString(StateCatch);
            if (string.IsNullOrEmpty(str))
                StatesColleciotn = new Dictionary<string, VirtualValue>();
            else
                StatesColleciotn = JsonConvert.DeserializeObject<Dictionary<string, VirtualValue>>(str);

            foreach (var state in PresetStates)
            {
                if (!StatesColleciotn.TryAdd(state.EventID, state.ValueType))
                {
                    if (state.ValueType != StatesColleciotn[state.EventID])
                    {
                        Debug.LogError(string.Format("{0} has two value type {1} and {2}. ", state.EventID, state.ValueType, StatesColleciotn[state.EventID]));
                        Errors.Add(state);
                    }
                }
            }
        }
        const string StateCatch = "StateValueTypeStorage";
        [BoxGroup(States)]
        [Button("Apply State m_Value Type")]
        void ApplyState()
        {
            var str = JsonConvert.SerializeObject(StatesColleciotn);
            EditorPrefs.SetString(StateCatch, str);
            foreach (var state in PresetStates)
            {
                if (StatesColleciotn.TryGetValue(state.EventID, out var valueType))
                {
                    state.SetValueType = valueType;
                    state.Dirty();
                }
            }
        }




        #endregion

        #region Wrapper

        const string WrapperGroup = "Wrapper";

        public const string WrapperID = "StateEditorWrapperID";

        [ShowInInspector]
        [BoxGroup(WrapperGroup)]
        public string WrapperName
        {
            get => EditorPrefs.GetString(WrapperID);
            set => EditorPrefs.SetString(WrapperID, value);
        }
        [BoxGroup(WrapperGroup)]
        [ShowInInspector]
        public Token Token;

        [BoxGroup(WrapperGroup)]
        [Button("GenerateWrapper")]
        void Generate()
        {
            var dic = PresetStates.ToDictionary(x => x.EventID);
            foreach (var state in StatesColleciotn)
            {
                var wrapper = EditorAssetsUtility.CreateInstanceSO<StateWrapper>(false, Path, WrapperName + "_" + state.Key + ".asset");
                if (dic.TryGetValue(state.Key, out var motionState))
                {
                    wrapper.Title = state.Key;
                    wrapper.State = motionState;
                }


                if (Token)
                    wrapper.Token = Token;

                wrapper.Dirty();
            }
        }
        #endregion


    }
}