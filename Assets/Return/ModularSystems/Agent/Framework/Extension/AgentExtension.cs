using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Agents
{
    public static class AgentExtension
    {
        public static bool IsLocalUser(this IAgent agent)
        {
            if (agent.Resolver.TryGetModule<TNet.ITNO>(out var tno))
                return tno.isMine;

            return false;
        }

        public static bool TryGetAgent<T>(this Collider collider,out T agent) where T:Component,IAgent
        {
            var root = collider.GetRoot(Agent.Tag);

            if(root==null)
            {
                agent = null;
                return false;
            }   
            
            return root.TryGetComponent<T>(out agent);
        }
    }
}
