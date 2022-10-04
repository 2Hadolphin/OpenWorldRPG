using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Sirenix.OdinInspector;


namespace Return
{
    [DefaultExecutionOrder(ExecuteOrderList.Framework)]
    public class ManagerLoader : BaseComponent
    {
        [AssetsOnly]
        [ShowInInspector]
        public List<Singleton_SO> PreloadManagers;


        private void Awake()
        {
            var sb = new StringBuilder();

            foreach (var manager in PreloadManagers)
            {
                sb.AppendLine(manager.name);
                manager.Initialize();
            }

            Debug.Log(sb.ToString());
        }
    }
}
