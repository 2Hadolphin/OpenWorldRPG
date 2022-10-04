using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization.Formatters
{
    public class AnimatorFormatter : NeoSerializationFormatter<Animator>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void Register()
        {
            NeoSerializationFormatters.RegisterFormatter(new AnimatorFormatter());
        }

        private readonly static NeoSerializationKey k_HashKey = new NeoSerializationKey("hash");
        private readonly static NeoSerializationKey k_TimeKey = new NeoSerializationKey("time");
        private readonly static NeoSerializationKey k_WeightKey = new NeoSerializationKey("weight");
        private readonly static NeoSerializationKey k_TransitionToKey = new NeoSerializationKey("toHash");
        private readonly static NeoSerializationKey k_TransitionTimeKey = new NeoSerializationKey("toTime");
        private readonly static NeoSerializationKey k_TransitionDurationKey = new NeoSerializationKey("toDuration");

        protected override void WriteProperties(INeoSerializer writer, Animator from, NeoSerializedGameObject nsgo)
        {
            if (!from.isActiveAndEnabled)
                return;

            // Write parameters (except triggers)
            writer.PushContext(SerializationContext.ObjectNeoFormatted, -1);
            for (int i = 0; i < from.parameterCount; ++i)
            {
                var p = from.GetParameter(i);
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        writer.WriteValue(p.nameHash, from.GetBool(p.nameHash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        writer.WriteValue(p.nameHash, from.GetInteger(p.nameHash));
                        break;
                    case AnimatorControllerParameterType.Float:
                        writer.WriteValue(p.nameHash, from.GetFloat(p.nameHash));
                        break;
                }
            }
            writer.PopContext(SerializationContext.ObjectNeoFormatted);
            
            // Write layer status
            for (int i = 0; i < from.layerCount; ++i)
            {
                float layerWeight = from.GetLayerWeight(i);

                writer.PushContext(SerializationContext.ObjectNeoFormatted, i);

                var stateInfo = from.GetCurrentAnimatorStateInfo(i);
                writer.WriteValue(k_HashKey, stateInfo.fullPathHash);
                writer.WriteValue(k_TimeKey, stateInfo.normalizedTime - (int)stateInfo.normalizedTime);
                writer.WriteValue(k_WeightKey, layerWeight);

                // Check for transition
                if (from.IsInTransition(i))
                {
                    var nextState = from.GetNextAnimatorStateInfo(i);
                    writer.WriteValue(k_TransitionToKey, nextState.fullPathHash);
                    writer.WriteValue(k_TransitionTimeKey, nextState.normalizedTime - (int)nextState.normalizedTime);

                    var transition = from.GetAnimatorTransitionInfo(i);
                    writer.WriteValue(k_TransitionDurationKey, transition.duration * 0.01f * (100f - transition.normalizedTime));
                }

                writer.PopContext(SerializationContext.ObjectNeoFormatted);
            }
        }
        
        protected override void ReadProperties(INeoDeserializer reader, Animator to, NeoSerializedGameObject nsgo)
        {
            if (!to.isActiveAndEnabled)
                return;

            // Read parameters
            if (reader.PushContext(SerializationContext.ObjectNeoFormatted, -1))
            {
                for (int i = 0; i < to.parameterCount; ++i)
                {
                    var p = to.GetParameter(i);
                    switch (p.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            {
                                bool value;
                                if (reader.TryReadValue(p.nameHash, out value, false))
                                    to.SetBool(p.nameHash, value);
                            }
                            break;
                        case AnimatorControllerParameterType.Int:
                            {
                                int value;
                                if (reader.TryReadValue(p.nameHash, out value, 0))
                                    to.SetInteger(p.nameHash, value);
                            }
                            break;
                        case AnimatorControllerParameterType.Float:
                            {
                                float value;
                                if (reader.TryReadValue(p.nameHash, out value, 0f))
                                    to.SetFloat(p.nameHash, value);
                            }
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            to.ResetTrigger(p.nameHash);
                            break;
                    }
                }
                reader.PopContext(SerializationContext.ObjectNeoFormatted, -1);
            }

            // Read layer status
            for (int i = to.layerCount - 1; i >= 0; --i)
            {
                // Push context if it exists (weight was 0 if not)
                if (reader.PushContext(SerializationContext.ObjectNeoFormatted, i))
                {
                    // Read layer weight
                    float w = 0f;
                    if (reader.TryReadValue(k_WeightKey, out w, 0f))
                        to.SetLayerWeight(i, w);
                    // Read layer state
                    int hash = 0; float time = 0f;
                    if (reader.TryReadValue(k_HashKey, out hash, 0) && reader.TryReadValue(k_TimeKey, out time, 0f))
                    {
                        to.Play(hash, i, time);
                    }
                    // Read transition
                    if (reader.TryReadValue(k_TransitionToKey, out hash, 0) && reader.TryReadValue(k_TransitionTimeKey, out time, 0f))
                    {
                        float fadeDuration;
                        reader.TryReadValue(k_TransitionDurationKey, out fadeDuration, 0.25f);
                        to.CrossFade(hash, fadeDuration, i, time);
                    }
                    // Pop context
                    reader.PopContext(SerializationContext.ObjectNeoFormatted, i);
                }
                else
                    to.SetLayerWeight(i, 0f);
            }
        }
    }
}