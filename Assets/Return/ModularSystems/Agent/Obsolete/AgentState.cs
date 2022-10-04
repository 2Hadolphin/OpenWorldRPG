using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Definition;

public class AgentState : Health
{
    private int go = 1;

    [SerializeField]
    private CommonData Identity;
    [SerializeField]
    private RigData Address;
    [SerializeField]
    private CreatureData PhysicalData;
    [SerializeField]
    private AgentData PlayerAbility;

    public int Strength { get { return go; } }
    public int Endurance { get { return go; } }
    public int BastBandwidth { get { return go; } }
    public int Intelligence { get { return go; } }
    public int LearningCoefficient { get { return go; } }
    public int Program { get { return go; } }
    public int Agility { get { return go; } }
    public int Dexterity { get { return go; } }
    /// <summary>
    /// 0-1 : 0-90
    /// </summary>
    public int Insight { get { return go; } }
    public int Faith { get { return go; } }
    public int Individual { get { return go; } }
    public int Pack { get { return go; } }
    public int Luck { get { return go; } }




}
