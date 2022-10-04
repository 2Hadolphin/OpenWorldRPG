using Return.Agents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Sirenix.OdinInspector;

namespace Return.Items
{
    public class Item : AbstractItem
    {

        protected virtual void LoadModules(IEnumerable<object> modules)
        {
            foreach (var module in modules)
            {
                try
                {
                    if (module is GameObject go)
                    {
                        LoadModules(go.GetComponents<MonoBehaviour>());
                        continue;
                    }


                    resolver.RegisterModule(module);

                    // register handler
                    if (module is IItemModule itemModule)
                    {
                        itemModule.SetHandler(this);
                        itemModule.Register();
                    }
    

                    Debug.LogFormat("Load {0} preset {1}", module, module);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

        } 
    }
}