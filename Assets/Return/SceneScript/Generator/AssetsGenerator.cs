using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.SceneModule
{
    public abstract class AssetsGenerator : MonoBehaviour
    {
        public static GeneratorManager manager;
        public static AssetsPool pool;
        public virtual void Initialization()
        {

        }
        public abstract void Generate();
    }
}