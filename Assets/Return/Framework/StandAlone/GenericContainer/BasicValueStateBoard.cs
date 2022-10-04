using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System;
using System.Collections;
using Sirenix.OdinInspector;

namespace Return
{
    public class TrashStateBoard : PresetDatabase
    {
        private void Awake()
        {
            Subscribes = new Dictionary<string, int>();
            var values = (int[])Enum.GetValues(typeof(VirtualValue));
         
            foreach (var value in values)
                Subscribes.Add(((VirtualValue)value).ToString(), 0);
        }

        /// <summary>
        /// Create the state if state doesn't exist
        /// </summary>
        public virtual void RegisterGetter(StateWrapper wrapper)
        {
            Assert.IsNotNull(wrapper);
            Subscribes[wrapper.State.ValueType.ToString()]++;

            if (wrapper.State is VirtualBlackBoard @virtualBoard)
            {
                switch (virtualBoard.ValueType)
                {
                    case VirtualValue.Bool:
                        mBool.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Float:
                        mFloat.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Int:
                        mInteger.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Vector2:
                        mVector2.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Vector3:
                        mVector3.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Vector4:
                        mVector4.CheckAdd(wrapper);
                        break;
                    case VirtualValue.Quaternion:
                        mQuaternion.CheckAdd(wrapper, Quaternion.identity);
                        break;
                    case VirtualValue.Trigger:
                        mTrigger.CheckAdd(wrapper, new UEvent<bool>(this));
                        break;
                    case VirtualValue.Generic:
                        break;
                }
            }
        }

        public virtual void RegisterGetters(params StateWrapper[] wrappers)
        {
            var length = wrappers.Length;
            for (int i = 0; i < length; i++)
                RegisterGetter(wrappers[i]);
        }

        /// <summary>
        /// Create the state if state doesn't exist and set wrapper edit qualify
        /// </summary>
        public virtual bool RegisterSetter(StateWrapper wrapper)
        {
            RegisterGetter(wrapper);
            
            var qualify = QualifySetter.Register(wrapper, out var lastToken);
            if (!qualify)
                Debug.Log(lastToken + "-" + wrapper.Token);
            //if (qualify)
            {
                //if (lastToken)
                //    if(_QualifySetter.Remove(lastToken));

                if (!shortQualify.Add(wrapper))
                    return false;
                    //Debug.LogError(string.Format("AbstractValue Board shortQualify qualify : {0} wrapper : {1}", qualify, wrapper));

                var state = wrapper.State;
                switch (state.ValueType)
                {
                    case VirtualValue.Bool:
                        if (state is GenericBlackBoard<bool> b)
                            SetBool(wrapper, b);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Float:
                        if (state is GenericBlackBoard<float> f)
                            SetFloat(wrapper, f);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Int:
                        if (state is GenericBlackBoard<int> i)
                            SetInt(wrapper, i);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector2:
                        if (state is GenericBlackBoard<Vector2> v2)
                            SetVector2(wrapper, v2);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector3:
                        if (state is GenericBlackBoard<Vector3> v3)
                            SetVector3(wrapper, v3);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector4:
                        if (state is GenericBlackBoard<Vector4> v4)
                            SetVector4(wrapper, v4);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Quaternion:
                        if (state is GenericBlackBoard<Quaternion> q)
                            SetQuaternion(wrapper, q);
                        else
                            SetBool(wrapper, default);
                        break;
                }
            }

            return qualify;
        }

        public virtual void Unregister(StateWrapper wrapper)
        {
            if (Subscribes.ContainsKey(wrapper))
                Subscribes[wrapper.State.EventID]--;
            //else
            //    Debug.LogError("Missing wrapper : " + wrapper.name);
        }



        [ReadOnly, ShowInInspector]
        protected HashSet<StateWrapper> QualifySetter = new();
        [ReadOnly, ShowInInspector]
        protected HashSet<int> shortQualify = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, int> Subscribes;
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, UEvent<bool>> mTrigger = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, bool> mBool = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, float> mFloat = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, int> mInteger = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, Vector2> mVector2 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, Vector3> mVector3 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, Vector4> mVector4 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, Quaternion> mQuaternion = new();

        public virtual bool GetBool(StateWrapper hash)
        {
            if (mBool.TryGetValue(hash, out bool value))
                return value;            else throw new KeyNotFoundException(hash.State.name);
        }

        public virtual UEvent<bool> GetTrigger(StateWrapper hash)
        {
            if (mTrigger.TryGetValue(hash, out UEvent<bool> value))
                return value;            
            else 
                throw new KeyNotFoundException(hash.State.name);
        }




        public virtual float GetFloat(StateWrapper hash)
        {
            if(mFloat.TryGetValue(hash, out float value))
                return value;            
            else
                throw new KeyNotFoundException(hash.State.name);
        }
        public virtual int GetInt(StateWrapper hash)
        {
            if(mInteger.TryGetValue(hash, out int value))
                return value;          
            else 
                throw new KeyNotFoundException(hash.State.name);
        }
        public virtual Vector2 GetVector2(StateWrapper hash)
        {
            if(mVector2.TryGetValue(hash, out Vector2 value))
                return value;          
            else 
                throw new KeyNotFoundException(hash.State.name);
        }

        public virtual Vector3 GetVector3(StateWrapper hash)
        {
            if(mVector3.TryGetValue(hash.State.EventID, out Vector3 value))
                return value;          
            else
                throw new KeyNotFoundException(hash.State.name);
        }

        public virtual Vector4 GetVector4(StateWrapper hash)
        {
            if(mVector4.TryGetValue(hash, out Vector4 value))
                return value;            
            else 
                throw new KeyNotFoundException(hash.State.name);
        }

        public virtual Quaternion GetQuaternion(StateWrapper hash)
        {
            if(mQuaternion.TryGetValue(hash, out Quaternion value))
                return value;           
            else
                throw new KeyNotFoundException(hash.State.name);
        }

        public virtual void SetBool(StateWrapper wrapper, bool value=true)
        {
            if (shortQualify.Contains(wrapper))
                mBool[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }

        public virtual void SetTrigger(StateWrapper wrapper, bool value)
        {
            if (shortQualify.Contains(wrapper))
                mTrigger[wrapper].Invoke(this, value);
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }

        public virtual void SetFloat(StateWrapper wrapper, float value)
        {
            if (shortQualify.Contains(wrapper))
                mFloat[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }
        public virtual void SetInt(StateWrapper wrapper, int value)
        {
            if (shortQualify.Contains(wrapper))
                mInteger[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }
        public virtual void SetVector2(StateWrapper wrapper, Vector2 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector2[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }

        public virtual void SetVector3(StateWrapper wrapper, Vector3 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector3[wrapper.State.EventID] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }

        public virtual void SetVector4(StateWrapper wrapper, Vector4 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector4[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);
        }

        public virtual void SetQuaternion(StateWrapper wrapper, Quaternion value)
        {
            if (shortQualify.Contains(wrapper))
                mQuaternion[wrapper] = value;
            else
                Debug.LogError("Invalid access " + wrapper.State.EventID);

        }



        //CharacterGroundingReport
        protected Dictionary<int, int> Generic = new();
        protected ArrayList mGeneric = new ArrayList(10);

        public virtual bool RegisterValueSetter<T>(StateWrapper wrapper, T value)
        {
            var qualify = QualifySetter.Register(wrapper, out var lastToken);

            if (qualify)
            {
                if (Generic.TryGetValue(wrapper.StateHash, out var sn))
                {
                    mGeneric[sn] = value;
                }
                else
                {
                    sn = mGeneric.Add(value);
                    Generic.Add(wrapper, sn);
                }

                //if (lastToken)
                //    if(_QualifySetter.Remove(lastToken));

                shortQualify.Add(wrapper);
            }

            return qualify;
        }



        public virtual T GetValue<T>(int hash)
        {
            if (Generic.TryGetValue(hash, out int sn))
                return (T)mGeneric[sn];
            else
                return default;
        }


        public virtual void SetValue<T>(StateWrapper wrapper, T value)
        {
            if(QualifySetter.Contains(wrapper))
            if (Generic.TryGetValue(wrapper, out int sn))
                mGeneric[sn] = value;
        }




    }


    public class BasicValueStateBoard:PresetDatabase
    {
        private void Awake()
        {
            Subscribes = new Dictionary<int, int>();
            var values = (int[])Enum.GetValues(typeof(VirtualValue));
            foreach (var value in values)
                Subscribes.Add(value, 0);
        }

        /// <summary>
        /// Create the state if state doesn't exist
        /// </summary>
        public virtual void RegisterGetter(StateWrapper wrapper)
        {
            Assert.IsNotNull(wrapper);
            Subscribes[(int)wrapper.State.ValueType] ++;
            if (wrapper.State is VirtualBlackBoard @virtualBoard)
            {
                switch (virtualBoard.ValueType)
                {
                    case VirtualValue.Bool:
                        mBool.CheckAdd(wrapper.StateHash);
                        break;
                    case VirtualValue.Float:
                        mFloat.CheckAdd(wrapper.StateHash);
                        break;
                    case VirtualValue.Int:
                        mInteger.CheckAdd(wrapper.StateHash);
                        break;
                    case VirtualValue.Vector2:
                        mVector2.CheckAdd(wrapper.StateHash);
                        break;
                    case VirtualValue.Vector3:
                        mVector3.CheckAdd(wrapper.State.EventID);
                        break;
                    case VirtualValue.Vector4:
                        mVector4.CheckAdd(wrapper.StateHash);
                        break;
                    case VirtualValue.Quaternion:
                        mQuaternion.CheckAdd(wrapper.StateHash,Quaternion.identity);
                        break;
                    case VirtualValue.Trigger:
                        mTrigger.CheckAdd(wrapper.StateHash, new UEvent<bool>(this));
                        break;
                    case VirtualValue.Generic:
                        break;
                }
            }
        }

        /// <summary>
        /// Create the state if state doesn't exist and set wrapper edit qualify
        /// </summary>
        public virtual bool RegisterSetter(StateWrapper wrapper)
        {
            RegisterGetter(wrapper);
            var qualify= QualifySetter.Register(wrapper,out var lastToken);
            if (!qualify)
                Debug.Log(lastToken + "-" + wrapper.Token);
            //if (qualify)
            {
                //if (lastToken)
                //    if(_QualifySetter.Remove(lastToken));

                if (!shortQualify.Add(wrapper))
                    Debug.LogError(qualify+"-"+wrapper);

                var state = wrapper.State;
                switch (state.ValueType)
                {
                    case VirtualValue.Bool:
                        if (state is GenericBlackBoard<bool> b)
                            SetBool(wrapper, b);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Float:
                        if (state is GenericBlackBoard<float> f)
                            SetFloat(wrapper, f);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Int:
                        if (state is GenericBlackBoard<int> i)
                            SetInt(wrapper, i);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector2:
                        if (state is GenericBlackBoard<Vector2> v2)
                            SetVector2(wrapper, v2);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector3:
                        if (state is GenericBlackBoard<Vector3> v3)
                            SetVector3(wrapper, v3);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Vector4:
                        if (state is GenericBlackBoard<Vector4> v4)
                            SetVector4(wrapper, v4);
                        else
                            SetBool(wrapper, default);
                        break;
                    case VirtualValue.Quaternion:
                        if (state is GenericBlackBoard<Quaternion> q)
                            SetQuaternion(wrapper, q);
                        else
                            SetBool(wrapper, default);
                        break;
                 }
            }

            return qualify;
        }

        public virtual void Unregister(StateWrapper wrapper)
        {
            Subscribes[(int)wrapper.State.ValueType]--;
        }

        [ReadOnly,ShowInInspector]
        protected HashSet<StateWrapper> QualifySetter = new();
        [ReadOnly, ShowInInspector]
        protected HashSet<int> shortQualify = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, int> Subscribes;
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, UEvent<bool>> mTrigger = new();
        [ReadOnly, ShowInInspector] 
        protected Dictionary<int, bool> mBool = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, float> mFloat = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, int> mInteger = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, Vector2> mVector2 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<string, Vector3> mVector3 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, Vector4> mVector4 = new();
        [ReadOnly, ShowInInspector]
        protected Dictionary<int, Quaternion> mQuaternion = new();

        public virtual bool GetBool(int hash)
        {
                if(mBool.TryGetValue(hash, out bool value))
                    return value;            else throw new KeyNotFoundException(hash.ToString());
        }

        public virtual UEvent<bool> GetTrigger(int hash)
        {
            if(mTrigger.TryGetValue(hash, out UEvent<bool> value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }

        public virtual float GetFloat(int hash)
        {
            if(mFloat.TryGetValue(hash, out float value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }
        public virtual int GetInt(int hash)
        {
            if(mInteger.TryGetValue(hash, out int value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }
        public virtual Vector2 GetVector2(int hash)
        {
            if(mVector2.TryGetValue(hash, out Vector2 value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }

        public virtual Vector3 GetVector3(StateWrapper hash)
        {
            if(mVector3.TryGetValue(hash.State.EventID, out Vector3 value))
                return value;            else throw new KeyNotFoundException(hash.State.name);
        }

        public virtual Vector4 GetVector4(int hash)
        {
            if(mVector4.TryGetValue(hash, out Vector4 value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }

        public virtual Quaternion GetQuaternion(int hash)
        {
            if(mQuaternion.TryGetValue(hash, out Quaternion value))
                return value;            else throw new KeyNotFoundException(hash.ToString());
        }

        public virtual void SetBool(StateWrapper wrapper, bool value)
        {
            if (shortQualify.Contains(wrapper))
                mBool[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }

        public virtual void SetTrigger(StateWrapper wrapper, bool value)
        {
            if (shortQualify.Contains(wrapper))
                mTrigger[wrapper.StateHash].Invoke(this, value);
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }

        public virtual void SetFloat(StateWrapper wrapper,float value)
        {
            if (shortQualify.Contains(wrapper))
                mFloat[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }
        public virtual void SetInt(StateWrapper wrapper, int value)
        {
            if (shortQualify.Contains(wrapper))
                mInteger[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }
        public virtual void SetVector2(StateWrapper wrapper, Vector2 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector2[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }

        public virtual void SetVector3(StateWrapper wrapper, Vector3 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector3[wrapper.State.EventID] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }

        public virtual void SetVector4(StateWrapper wrapper, Vector4 value)
        {
            if (shortQualify.Contains(wrapper))
                mVector4[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);
        }

        public virtual void SetQuaternion(StateWrapper wrapper, Quaternion value)
        {
            if (shortQualify.Contains(wrapper))
                mQuaternion[wrapper.StateHash] = value;
            else
                Debug.LogError("Invalid access" + wrapper.State.EventID);

        }



        //CharacterGroundingReport
        protected Dictionary<int, int> Generic = new();
        protected ArrayList mGeneric = new ArrayList(10);

        public virtual bool RegisterValueSetter<T>(StateWrapper wrapper, T value)
        {
            var qualify = QualifySetter.Register(wrapper, out var lastToken);

            if (qualify)
            {
                if (Generic.TryGetValue(wrapper.StateHash, out var sn))
                {
                    mGeneric[sn] = value;
                }
                else
                {
                    sn = mGeneric.Add(value);
                    Generic.Add(wrapper, sn);
                }

                //if (lastToken)
                //    if(_QualifySetter.Remove(lastToken));

                shortQualify.Add(wrapper);
            }

            return qualify;
        }



        public virtual T GetValue<T>(int hash)
        {
            if (Generic.TryGetValue(hash, out int sn))
                return (T)mGeneric[sn];
            else
                return default;
        }


        public virtual void SetValue<T>(StateWrapper wrapper,T value) 
        {
            if(QualifySetter.Contains(wrapper))
            if (Generic.TryGetValue(wrapper, out int sn))
                mGeneric[sn] = value;
        }




    }

}