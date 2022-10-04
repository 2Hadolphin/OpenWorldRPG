using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Return.Humanoid;
using Return;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;

namespace Return.Motions
{

    /// <summary>
    /// Cache all motion state ??
    /// </summary>
    [Serializable]
    public class MotionStateBundle : PresetDatabase
    {
        //[ShowInInspector]
        //public ObsoleteMotionState[] States=new ObsoleteMotionState[0];




#if UNITY_EDITOR


        public List<MotionState> ToAdd;
        [Button("AddStates")]
        void AddStates()
        {
            foreach (var state in ToAdd)
            {
                AddState(state);
            }
        }


        void AddState(MotionState state)
        {
            if (AssetDatabase.IsMainAsset(state))
            {
                var path = AssetDatabase.GetAssetPath(state);
                AssetDatabase.RemoveObjectFromAsset(state);
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.AddObjectToAsset(state, AssetDatabase.GetAssetPath(this));
            AssetDatabase.SaveAssets();
        }

        private void OnEnable()
        {
            m_States = Editors.EditorAssetsUtility.GetSubObjectsOfType<MotionState>(this).ToList();
            
        }

        void RemoveState(MotionState state)
        {
            m_States.Remove(state);
            Return.Editors.EditorAssetsUtility.DeleteSubAsset(state, false, false);
        }

        [ListDrawerSettings(CustomRemoveElementFunction = nameof(RemoveState))]
#endif
        [SerializeField]
        [Searchable]
        public List<MotionState> m_States;

        public Token SystemToken;
        public Token ModuleToken;


#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="stateAttribute"></param>
        /// <param name="isModule"></param>
        public void BindWrapper(SerializedProperty property, VirtualStateAttribute stateAttribute, bool isModule,Func<StateWrapper> builder)
        {
            var dic = new Dictionary<string, StateWrapper>();
            var targetObj = property.serializedObject.targetObject;
            var requireSet = stateAttribute.Writable;

            var obs = Editors.EditorAssetsUtility.GetSubObjectsOfType<StateWrapper>(targetObj);

            foreach (var obj in obs)
            {
                if (obj.State)
                    if (obj.name == obj.State.EventID)
                        dic.SafeAdd(obj.name, obj);
            }

            {
                var stateID = property.name;
                if (dic.TryGetValue(stateID, out var wrapper))
                {
                    property.objectReferenceValue = wrapper;
                }
                else
                {

                    Debug.Log(string.Format("Can't found any state wrapper with {0} event id.", property.name));
                    {
                        wrapper = builder();
                    }
                }

                if (!wrapper.State)
                {
                    var type = stateAttribute.ValueType;

                    var title = stateID;

                    MotionState state = null;

                    foreach (MotionState mState in m_States)
                    {
                        if (mState.EventID == stateID)
                        {
                            state = mState;
                            break;
                        }
                    }

                    if (!state)
                    {
                        state = CreateInstance<MotionState>();
                        state.Title = title;
                        state.EventID = stateID;
                        state.SetValueType = type;
                        state.name = stateID;
                        AddState(state);
                        //state.Dirty();
                    }

                    wrapper.State = state;
                    wrapper.Title = title;
                }

                if (!wrapper.Token)
                {
                    var token = isModule ? ModuleToken : SystemToken;

                    if (requireSet)
                    {
                        wrapper.Token = token;
                        wrapper.Dirty();
                    }
                }
            }
        }

#endif
    }
}