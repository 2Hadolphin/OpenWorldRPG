using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public abstract class BasePlayableBehaviour : PlayableBehaviour
{
    public PlayableGraph m_Graph;
    public Playable Self { get; protected set; }

    public static ScriptPlayable<T> Create<T>(PlayableGraph graph,int input=0) where T: BasePlayableBehaviour,new ()
    {
        var sp = ScriptPlayable<T>.Create(graph, input);

        var behaviour = sp.GetBehaviour();
        behaviour.m_Graph = graph;
        behaviour.Self = sp;

        return sp;
    } 



}
