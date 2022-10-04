using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Cameras;
using Cysharp.Threading.Tasks;

// TODO Survival Tower Defense Game
namespace Return.Games.AlphaMap
{
    /// <summary>
    /// SetHandler game modules in scene. **Spawn **Score **
    /// </summary>
    public class GameLoader : BaseComponent,IStart
    {
        async void IStart.Initialize()
        {

            await UniTask.DelayFrame(5);

            var cam = CameraManager.GetCamera<FreeCamera>();

            if(cam)
                cam.enabled = true;
        }

    }
}
