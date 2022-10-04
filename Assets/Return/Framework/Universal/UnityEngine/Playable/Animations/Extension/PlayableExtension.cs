using UnityEngine.Animations;
using Unity.Collections;
using System;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine;
using System.Collections.Generic;
using Return.Animations;
using System.Text;

public class PlayableUtil
{
    public static int IndexOfType<V, U>(U playable) where U : struct, IPlayable where V : struct, IPlayable
    {
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInput(i).IsPlayableOfType<V>())
                return i;
        }
        return -1;
    }

}

public static class PlayableExtension
{

    /// <summary>
    /// CreatePlayableBehaviour if is invalid
    /// </summary>
    public static void CreateBehaviour<T>(this PlayableGraph graph, ref ScriptPlayable<T> playableBehaviour, int inputCount = 0) where T : BasePlayableBehaviour, new()
    {
        if (playableBehaviour.IsValid())
            return;

        playableBehaviour = BasePlayableBehaviour.Create<T>(graph, inputCount);
    }



    public static bool Parse<T2>(this Playable value, out T2 result) where T2 : struct, IPlayable
    {
        var valid = value.IsPlayableOfType<T2>();

        result = valid ? (T2)(object)value : (T2)(object)Playable.Null;

        return valid;
    }
    public static float GetTimeLeft<T>(this T playable) where T : struct, IPlayable
    {
        return playable.GetDurationF() - (float)playable.GetTime();
    }

    public static float GetRatio<T>(this T playable) where T : struct, IPlayable
    {
        if (playable.IsDone())
            return 1f;

        float ratio = (float)(playable.GetTime()/playable.GetDuration());

#if UNITY_EDITOR

        if (float.IsNaN(ratio))
            throw new Exception("Ratio is NaN");

        if (float.IsInfinity(ratio))
            throw new Exception("Ratio is Infinity");
#endif

        return Mathf.Clamp01(ratio);
    }

    /// <summary>
    /// Destory this playable after checking null.
    /// </summary>
    public static void SafeDestory<T>(this T playable) where T : struct, IPlayable
    {
        if (!playable.IsNull() && playable.IsValid() && playable.CanDestroy())
            playable.Destroy();
        else
            Debug.LogWarning(
                $"Playable is invalid, will not be destroy." +
                $"\nNull : {playable.IsNull()}" +
                $"\nValid : {playable.IsValid()}" +
                $"\nType : {typeof(T)}");
    }

    public static IEnumerable<Playable> GetInputs<T>(this T playable,bool checkValid=true)where T : struct, IPlayable
    {
        var length = playable.GetInputCount();

        for (int i = 0; i < length; i++)
        {
            var input = playable.GetInput(i);

            if (input.IsQualify())
                yield return input;
        }
    }

    public static IEnumerable<Playable> TraversUpward<T>(this T playable, bool checkValid = true) where T : struct, IPlayable
    {
        if (playable.IsNull())
        {
            Debug.LogError("Null "+playable);
            yield break;
        }
 
        if (!playable.IsValid())
        {
            if(playable is Playable aa)
                Debug.LogError("Valid " + aa.GetPlayableType());
            yield break;
        }

        var length = playable.GetInputCount();

        for (int i = 0; i < length; i++)
        {
            var input = playable.GetInput(i);

            if (input.IsQualify())
            {
                var inputs = input.TraversUpward(checkValid);//.GetEnumerator();

                foreach (var curInput in inputs)
                    yield return curInput;
            }
        }

        if(playable is Playable pb)
            yield return pb;

        yield break;
    }

    public static void DestoryUpward<T>(this T playable,bool destoryThis=true) where T : struct, IPlayable
    {
        Assert.IsFalse(playable.IsNull());

        var playables = playable.TraversUpward(false);

        foreach (var toDestory in playables)
        {
            Assert.IsTrue(toDestory.CanDestroy());

            if (!destoryThis&& toDestory.GetHandle().Equals(playable.GetHandle()))
               continue;

            toDestory.Destroy();
        }
    }


    public static IEnumerable<Playable> GetInputs(this Playable playable)
    {
        var count = playable.GetInputCount();
        for (int i = 0; i < count; i++)
        {
            yield return playable.GetInput(i);
        }
        yield break;
    }


    #region PlayableGraph

    public static void DisconnectInputs(this Playable playable, params int[] ports)
    {
        if (ports == null || ports.Length == 0)
        {
            var length = playable.GetInputCount();
            for (int i = length - 1; i >= 0; i--)
                playable.DisconnectInput(i);
        }
        else
        {
            var length = ports.Length;
            for (int i = 0; i < length; i++)
                playable.DisconnectInput(ports[i]);
        }
    }

    /// <summary>
    /// Insert target playable between from and to
    /// </summary>
    public static void InsertPlayable(this Playable to, Playable from, Playable target,int targetPort=0)
    {
        try
        {
            Assert.IsFalse(to.IsNull());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        try
        {
            Assert.IsFalse(from.IsNull());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        try
        {
            Assert.IsFalse(target.IsNull());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        var inputIndex = to.IndexOfInput(from);

        if (inputIndex < 0)
            inputIndex = 0;

        var outputIndex = from.IndexOfOutput(to);
        if (outputIndex < 0)
            outputIndex = 0;

        to.DisconnectInput(from);

        if (target.GetInputCount() == 0)
            target.SetInputCount(1);

        if (target.GetOutputCount() == 0)
            target.SetOutputCount(1);

        target.ConnectInput(0, from, outputIndex, 1);
        to.ConnectInput(inputIndex, target, 0, 1);
    }

    public static void Insert(this AnimationPlayableOutput output,Playable playable)
    {
        var source = output.GetSourcePlayable();

        playable.AddInput(source,0,1);

        output.SetSourcePlayable(playable);

    }

    public static int GetInputIndex(this Playable root, Playable target)
    {
        var length = root.GetInputCount();

        for (int i = 0; i < length; i++)
            if (root.GetInput(i).Equals(target))
                return i;

        return -1;
    }

    public static int GetInputIndexOfType<T>(this Playable root) where T : struct, IPlayable
    {
        var newPortCount = root.GetInputCount();
        for (int i = 0; i < newPortCount; i++)
        {
            if (root.GetInput(i).IsPlayableOfType<T>())
                return i;
        }

        return -1;
    }

    public static Playable ReplaceInput(this Playable root, Playable newInputPlayable, int rootInputPort = 0, int inputPlayableOutputPort = 0, float weight = 1f)
    {
        var lastInput = root.GetInput(rootInputPort);
        root.DisconnectInput(rootInputPort);
        root.ConnectInput(rootInputPort, newInputPlayable, inputPlayableOutputPort, weight);
        return lastInput;
    }

    /// <summary>
    /// Return first inpunt playable.
    /// </summary>
    public static Playable FirstOrNull<T>(this T playable, float weightThreshold = float.MinValue) where T : struct, IPlayable
    {
        var count = playable.GetInputCount();

        bool threshold = weightThreshold > 0;

        for (int i = 0; i < count; i++)
        {
            var p = playable.GetInput(i);

            if (p.IsNull())
                continue;
            else if (threshold && playable.GetInputWeight(i) > weightThreshold)
                return p;
            else
                return p;
        }

        return Playable.Null;
    }

    /// <summary>
    /// Apply playable to a two port playable
    /// </summary>
    /// <param name="root">playable which handle two Input port</param>
    /// <param name="lastInput">last playable inside mixer if exist</param>
    /// <param name="newInput">new playable to connect to mixer</param>
    /// <param name="checkPort">CheckAdd the sourceIndex of port which match require condition</param>
    public static void SetNewInput_1D(this Playable root, Playable lastInput, Playable newInput, Func<Playable, bool> checkPort = null)
    {
        Assert.IsFalse(newInput.IsNull());

        if (lastInput.IsNull())
        {
            root.SetInputCount(1);

            if (checkPort != null) // limit type
            {
                var newPortCount = newInput.GetInputCount();
                for (int i = 0; i < newPortCount; i++)
                {
                    if (!checkPort(newInput.GetInput(i))) // check type
                        continue;

                    root.ConnectInput(0, newInput, i, 1);
                    return;
                }
            }
            else
                root.ConnectInput(0, newInput, 0, 1);
        }

        var lastIndex = root.GetInputIndex(lastInput);
        var newIndex = lastIndex < 1 ? 1 : 0;

        var rootInputCount = root.GetInputCount();

        if (rootInputCount == 2)
            root.ReplaceInput(newInput, newIndex);
        else
        {
            Debug.LogError(rootInputCount);

        }
    }

    public static void DisconnectOutputs(this Playable playable)
    {
        var length = playable.GetOutputCount();
        for (int i = 0; i < length; i++)
        {
            var outputPlayable = playable.GetOutput(i);
            if (!outputPlayable.IsNull())
                outputPlayable.DisconnectInput(playable);
        }
    }

    /// <summary>
    /// Whether playable contains Input from target
    /// </summary>
    public static bool HasInput<T>(this T playable, Playable target) where T : struct, IPlayable //where U : struct, IPlayable
    {
        try
        {
            Assert.IsFalse(playable.IsNull());
            Assert.IsFalse(target.IsNull());
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            return false;
        }

        var length = playable.GetInputCount();

        for (int i = 0; i < length; i++)
        {
            if (!playable.GetInput(i).Equals(target))
                continue;

            Debug.LogError("Repeat on " +( playable.GetInput(i).Equals(target))+target.GetPlayableType());
            //if (Playable.Equals(playable.GetInput(i), target))
            return true;
        }

        return false;
    }



    /// <summary>
    /// Whether playable contains Input from target
    /// </summary>
    [Obsolete]
    public static bool HasOutput<T,U>(this T playable, U target,bool searchChilds=false) where T:struct,IPlayable where U : struct, IPlayable
    {
        var length = playable.GetOutputCount();

        if (playable.GetHandle().Equals(target.GetHandle()))
            return true;

        for (int i = 0; i < length; i++)
        {
            var output = playable.GetOutput(i);
            Debug.Log(output.GetPlayableType()+" ** "+target);

            if (output.Equals(target))
                return true;
            else if (searchChilds)
            {
                if(output.IsQualify())
                    if (output.HasOutput(target, searchChilds))
                        return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Set all weights of this playable's output playable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="playable"></param>
    /// <param name="weight"></param>
    public static void SetOutputsWeight<T>(this T playable,float weight=0f) where T : struct, IPlayable
    {
        Assert.IsTrue(playable.IsQualify());

        var length = playable.GetOutputCount();
        var invalidCount = 0;

        for (int i = 0; i < length; i++)
        {
            var output = playable.GetOutput(i);

            if (output.IsNull())
            {
                if(playable is Playable pb)
                    Debug.LogError(pb.GetPlayableType()+"- output port is null : "+i);

                invalidCount++;
                continue;
            }

            output.SetInputWeight(playable, weight);
        }

        if(invalidCount>0)
            playable.SetOutputCount(length- invalidCount);
    }

    public static void SetInputsWeight<T>(this T playable, float weight = 0f) where T : struct, IPlayable
    {
        Assert.IsTrue(playable.IsValid());
        var inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            playable.SetInputWeight(i, weight);
        }
    }

    public static void CheckPlay<T>(this T playable) where T : struct, IPlayable
    {
        if (!playable.GetPlayState().HasFlag(PlayState.Playing))
            playable.Play();
    }

    /// <summary>
    /// Set time modue, time, speed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="playable"></param>
    /// <param name="setTime"></param>
    /// <param name="startTime"></param>
    /// <param name="speed"></param>
    public static void Init<T>(this T playable,bool setTime=true, double startTime = 0, double speed=1) where T : struct, IPlayable
    {
        playable.SetPropagateSetTime(setTime);
        playable.SetTime(startTime);
        playable.SetSpeed(speed);
    }

    /// <summary>
    /// CastExtension **Disconnect target playable if root playable has one.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="target"></param>
    public static void DisconnectInput<U, V>(this U root, V target) where U : struct, IPlayable where V : struct, IPlayable
    {
        Assert.IsFalse(target.IsNull());
        Assert.IsFalse(root.IsNull());

        var length = root.GetInputCount();
        for (int i = 0; i < length; i++)
        {
            if (target.Equals(root.GetInput(i)))
            {
                root.DisconnectInput(i);
                break;
            }
        }
    }

    /// <summary>
    /// Disconnect all Input ports ?? not delete?
    /// </summary>
    public static void Strip<U>(this U root,bool resetCount=true) where U : struct, IPlayable
    {
        var count = root.GetInputCount();

        int invalid = 0;

        for (int i = count-1; i > 0; i--)
        {
            var input = root.GetInput(i);

            if (input.IsNull() || !input.IsValid())
            {
                root.DisconnectInput(i);
                invalid++;
            }
        }
        
        if(resetCount)
            root.SetInputCount(count-invalid);
    }

    /// <summary>
    /// Return true if playable is not null and valid.
    /// </summary>
    public static bool IsQualify<V>(this V playable) where V : struct, IPlayable
    {
        return !playable.IsNull() && playable.IsValid();
    }

    /// <summary>
    /// Return true if time over duration.
    /// </summary>
    public static bool HasFinish<V>(this V playable) where V : struct, IPlayable
    {
        return Mathf.Approximately((float)playable.GetTime(), (float)playable.GetDuration());
    }

    /// <summary>
    /// Valid time and duration to set done.
    /// </summary>
    /// <param name="loop"></param>
    /// <returns>Return true if state change.</returns>
    public static bool ValidDone<T>(this T playable,bool loop = false ) where T : struct, IPlayable
    {
        var done = playable.IsDone();
        bool change = false;

        var duration = (float)playable.GetDuration();
        var time = (float)playable.GetTime();

        if (done)
        {
            if (time < duration && !Mathf.Approximately(time, duration))
            {
                change = true;
                playable.SetDone(false);
            }
        }
        else
        {
            if (loop)
                time %= duration;

            done = Mathf.Approximately(time, duration);

            if (done)
            {
                change = true;
                playable.SetDone(done);
            }
        }

        return change;
    }


    public static float GetDurationF<V>(this V playable) where V : struct, IPlayable
    {
        return (float)playable.GetDuration();
    }


    /// <summary>
    /// Return true if playable is playing.
    /// </summary>
    public static bool IsPlaying<V>(this V playable) where V : struct, IPlayable
    {
        return playable.GetPlayState().HasFlag(PlayState.Playing);
    }

    public static void DisconnectAllInput<T>(this T playable,bool destroy=true,bool resetCount=true) where T : struct, IPlayable
    {
        var count = playable.GetInputCount();

        for (int i = count - 1; i > 0; i--)
        {
            var input = playable.GetInput(i);

            if (!input.IsNull() && input.IsValid())
            {
                playable.DisconnectInput(i);
                input.Destroy();
            }
        }

        if (resetCount)
            playable.SetInputCount(0);
    }

    /// <summary>
    /// Return sourceIndex of Input sourcePlayable
    /// </summary>
    public static int IndexOfInput<U, V>(this U playable, V inputPlayable) where U : struct, IPlayable where V : struct, IPlayable
    {
        int inputCount = playable.GetInputCount();

        var sourceHandle = inputPlayable.GetHandle();
        for (int i = 0; i < inputCount; i++)
        {
            var input = playable.GetInput(i);
            if (input.IsQualify())
                if (input.GetHandle() == sourceHandle)
                    return i;
        }
        return -1;
    }

    /// <summary>
    /// Return sourceIndex of Input sourcePlayable
    /// </summary>
    public static int IndexOfOutput<U, V>(this U playable, V outputPlayable) where U : struct, IPlayable where V : struct, IPlayable
    {
        int outputCount = playable.GetOutputCount();

        var sourceHandle = outputPlayable.GetHandle();

        for (int i = 0; i < outputCount; i++)
        {
            var output=playable.GetOutput(i);
            if (output.IsQualify())
                if (output.GetHandle() == sourceHandle)
                    return i;
        }

        return -1;
    }

    public static IEnumerable<Playable>GetEnumerator<T>(this T playable) where T : struct, IPlayable
    {
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            yield return playable.GetInput(i);
        }

        yield break;
    }

    public static int IndexOfBehaviour<T>(this Playable playable) where T :class, IPlayableBehaviour
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            var intput = playable.GetInput(i);

            if (intput.IsNull())
                continue;
   

            if (intput.GetPlayableType().Equals(typeof(T)))
                return i;

            //Debug.LogError(i+" "+typeof(T) + " : " + intput.GetPlayableType());
        }

        return -1;
    }

    public static int IndexOfType<T>(this Playable playable) where T : struct, IPlayable
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            var intput = playable.GetInput(i);

            if (intput.IsNull())
                continue;
            //Debug.Log(string.Format("{4}/{5}\n GetPlayableType : {0}\nRequireType : {1}\nIsPlayableOfType : {2}\nIsCast : {3}",
            //playable.GetInput(i).GetPlayableType(),
            //typeof(T),
            //playable.GetInput(i).IsPlayableOfType<T>(),
            //playable.GetInput(i) is T,
            //i,
            //inputCount
            //));

            if (intput is T)
                return i;

            if (intput.GetPlayableType().Equals(typeof(T)))
                return i;

            if (intput.IsPlayableOfType<T>())
                return i;

            //Debug.LogError(typeof(T)+" : "+ intput.GetPlayableType());
        }

        //Debug.LogError("Can't found : " + typeof(T));
        return -1;
    }

    /// <summary>
    /// Return sourceIndex of Input with require type.
    /// </summary>
    public static int IndexOfType(this Playable playable,Type type)
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            //Debug.Log(string.Format("{3}/{4} GetPlayableType : {0}\nRequireType : {1}\nIsCast : {2}",
            //    playable.GetInput(i).GetPlayableType(),
            //    type.GetType(),
            //    playable.GetInput(i).GetType() == type,
            //    i,
            //    inputCount
            //    ));

            if (playable.GetInput(i).GetType() == type)
                return i;

            if (playable.GetInput(i).GetPlayableType()==type)
                return i;
        }

#if UNITY_EDITOR
        var sb = new StringBuilder("Can't found : " + type);
        for (int i = 0; i < inputCount; i++)
        {
            sb.AppendLine(playable.GetInput(i).GetPlayableType().ToString());
        }
        Debug.LogError(sb.ToString());
#endif

        return -1;
    }


    public static void SetPortOutput(this Playable playable, int sourceIndex)
    {
        if (playable.GetOutputCount() < sourceIndex + 1)
            playable.SetOutputCount(sourceIndex + 1);
    }



    public static int GetEmptyInputPort(this AnimationLayerMixerPlayable animationLayerMixer)
    {
        var length = animationLayerMixer.GetInputCount();
        for (int i = 0; i < length; i++)
        {
            if (animationLayerMixer.GetInput(i).IsNull())
                return i;
            else
                continue;
        }

        animationLayerMixer.SetInputCount(length + 1);

        return length;
    }



    //public static int AutoAddInput<T>(this Playable dst, Playable inputPlayable, float weight = 0, bool checkRepeat = false)
    //{
    //    if (checkRepeat && dst.HasInput(inputPlayable))
    //    {
    //        var busy = dst.IndexOfInput(inputPlayable);
    //        Debug.LogError(string.Format("Repeat binding Input : {0} at {1} port.", inputPlayable, busy));
    //        return -1;
    //    }

    //    return AutoAddInput<T>(dst, inputPlayable, weight);
    //}
    /// <summary>
    /// Connect Input with require type port
    /// </summary>
    /// <typeparam name="T">Never use gernic.</typeparam>
    /// <param name="dst"></param>
    /// <param name="inputPlayable"></param>
    /// <param name="weight"></param>
    /// <returns></returns>
    public static int AutoAddInput<T>(this Playable dst,Playable inputPlayable,float weight=0) where T : struct, IPlayable
    {
        var result = -1;
        int targetPort;

        //Debug.Log("Is SP : " + inputPlayable.GetType()+inputPlayable.GetPlayableType()+(inputPlayable.GetPlayableType() is IPlayableBehaviour));

        //if (typeof(T) is IPlayableBehaviour)
        //    targetPort = inputPlayable.IndexOfType(inputPlayable.GetPlayableType());
        //else
        //if(typeof(T) is IPlayableBehaviour)

        targetPort = inputPlayable.IndexOfType<T>();

        if (targetPort < 0)
            return result;
        else
            return AutoAddInput(dst, inputPlayable, targetPort, weight);

    }

    /// <summary>
    /// ??? Add Input with target port which match require type.
    /// </summary>
    public static int AutoAddInputGernic<Y>(this Playable dst, Playable inputPlayable, float weight = 0,bool checkRepeat=false) //where Y : class, IPlayableBehaviour, new()
    {
        var result = -1;

        if (checkRepeat && dst.HasInput(inputPlayable))
        {
            var busy = dst.IndexOfInput(inputPlayable);
            Debug.LogError(string.Format(typeof(Y)+" repeat binding Input : {0} at {1} port.", inputPlayable, busy));
            return result;
        }

        int targetPort;

        targetPort = inputPlayable.IndexOfType(typeof(Y));

        if (targetPort < 0)
        {
            Debug.LogError(string.Format("Can't found binding Input : {0} with {1}.", typeof(Y),inputPlayable.GetPlayableType()));
            return result;
        }
        else
            return AutoAddInput(dst, inputPlayable, targetPort, weight);
    }

    /// <summary>
    /// Safe add Input to last port.
    /// </summary>
    static int AutoAddInput(this Playable dst,Playable inputPlayable,int targetPort,float weight)
    {
        //Assert.IsTrue(dst.GetTraversalMode() == PlayableTraversalMode.Passthrough);

        var p = inputPlayable.GetOutputCount();

        if (p <= targetPort)
        {
            p = targetPort + 1;
            inputPlayable.SetOutputCount(p);
        }

        return dst.AddInput(inputPlayable, targetPort, weight);
    }

    #endregion




    #region Native

    public static void CleanMemory(this ref NativeArray<TransformStreamHandle> array)
    {
        if (array.IsCreated)
            array.Dispose();
    }

    #endregion
}

