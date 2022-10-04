using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Return
{
    /// <summary>
    /// Contains blackboard hash and provide priority token.
    /// </summary>
    [Serializable]
    public class StateWrapper :PresetDatabase,IEquatable<StateWrapper>
    {
        [OnValueChanged(nameof(LoadStateHash))]
        [SerializeField,Required]
        AbstractValue m_State;

        public AbstractValue State { get => m_State; set => m_State = value; }



        public Token Token;

        /// <summary>
        /// Global state hash
        /// </summary>
        [JsonIgnore][ShowInInspector][ReadOnly]
        public int StateHash { get; protected set; }


        private void OnEnable()
        {
            LoadStateHash();
        }

        void LoadStateHash()
        {
            if (State)
            {
                if (string.IsNullOrEmpty(Title))
                    Title = State.Title;

                if (string.IsNullOrEmpty(Description))
                    Description = State.Description;

                StateHash = State.GetHashCode();
            }
  
        }
#if UNITY_EDITOR
        void Reset()
        {
              LoadStateHash();
        }
#endif
        public bool Equals(StateWrapper other)
        {
            if(State && other)
                return State == other.State;
            else
                return false;
        }

        public static implicit operator int (StateWrapper wrapper)
        {
            return wrapper.StateHash;//+wrapper.GetHashCode();  
        }

        public static implicit operator BlackBoard (StateWrapper wrapper)
        {
            return wrapper.State;
        }

        public static implicit operator Token(StateWrapper wrapper)
        {
            return wrapper.Token;
        }

        public static implicit operator string(StateWrapper wrapper)
        {
            return wrapper.State.EventID;
        }


#if UNITY_EDITOR
        [ShowInInspector]
        [ReadOnly]
        /// <summary>
        /// EditorOnly
        /// </summary>
        public VirtualValue ValueType 
        { 
            get
            {
                if (State)
                    return State.ValueType;
                else
                    return default;
            } 
        }



        //[ReadOnly]
        //protected override void DescriptionField()
        //{
        //    if (State)
        //        GUILayout.TextArea(State._Description, GUILayout.Height(50));
        //    else
        //        base.DescriptionField();
        //}

#endif
    }

    public  static partial class mExtension
    {
        public static bool Register(this HashSet<StateWrapper> wrappers,StateWrapper wrapper,out Token lastToken)
        {
            if(wrappers.TryGetValue(wrapper,out var exist))
            {
                lastToken = exist.Token;

                if (lastToken == wrapper.Token)
                    return true;

                var order= wrapper.Token.CompareTo(lastToken);

                if (order > 0)
                    return false;

                //wrappers.Remove(exist);
                //wrappers.Add(wrapper);
            }
            else
            {
                wrappers.Add(wrapper);
                lastToken = null;
            }

            return true;
        }
    } 
}