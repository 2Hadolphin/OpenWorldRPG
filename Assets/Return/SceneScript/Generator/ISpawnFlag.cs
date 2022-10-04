using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnFlag
{
    SpawnFlag.Types GetSpawnType { get; }
    bool Ban { get; }

    void Activate();

    void CheckVisa();   // go wait deactivate or not
}
