using Return;
using UnityEngine;

public class ItemHandleSolver: ItemSolver
{
    protected Transform Handle;
    public override Vector3 HandlerPosition { get =>Handle.position; set =>Handle.position= value; }
}
