using System.Collections;
using UnityEngine;
using TNet;
using Steamworks;

public class NetState : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnGUI()
    {
        
        if (TNManager.isActive)
        {
            var rect = new Rect(Screen.width - 200, 0, 200, 100);
            GUILayout.BeginArea(rect);

            var channels = TNManager.channels;

            GUILayout.Label("Channels : "+ channels.Count.ToString());

            foreach (var channel in channels)
            {
                GUILayout.Label(channel.level);
            }

            GUILayout.EndArea();

            rect.y += 200;

            GUILayout.BeginArea(rect);
            {
                GUILayout.Label("LAN Server List");

                // List of discovered servers
                List<ServerList.Entry> list = LobbyClient.knownServers.list;

                // Server list example script automatically collects servers that have recently announced themselves
                for (int i = 0; i < list.size; ++i)
                {
                    var ent = list.buffer[i];

                    // NOTE: I am using 'internalAddress' here because I know all servers are hosted on LAN.
                    // If you are hosting outside of your LAN, you should probably use 'externalAddress' instead.
                    if (GUILayout.Button(ent.internalAddress.ToString()))
                    {
                        TNManager.Connect(ent.internalAddress, ent.internalAddress);
                    }

                    if (GUILayout.Button(ent.externalAddress.ToString()))
                    {
                        TNManager.Connect(ent.externalAddress, ent.externalAddress);
                    }
                }
            }
            GUILayout.EndArea();
        }
        else
        {

        }
    }
}
