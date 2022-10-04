using UnityEngine;
using System.IO;

namespace TNet
{

    public partial class GameServer
    {

		/// <summary>
		/// request custom packet.
		/// </summary>
		public void ProcessCustomPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, byte request, bool reliable)
		{
			var packet = (Packet)request;
            switch (packet)
            {
					case Packet.RequestCreateAddressalbeObject:
                    {
						Debug.Log($"On {packet}");

						var playerID = reader.ReadInt32();

						// Exploit: echoed packet of another player
						if (playerID != player.id)
						{
							player.LogError("Tried to echo a create packet (" + playerID + " vs " + player.id + ")", null);
							RemovePlayer(player);
							return;
						}

						var channelID = reader.ReadInt32();
						var ch = player.GetChannel(channelID);
						var type = reader.ReadByte();

						if (ch != null && (!ch.isLocked || player.isAdmin))
						{
							uint uniqueID = 0;

							if (type != 0)
							{
								uniqueID = ch.GetUniqueID();

                                var obj = new Channel.CreatedObject
                                {
                                    playerID = player.id,
                                    objectID = uniqueID,
                                    type = type
                                };

                                if (buffer.size > 0)
								{
									buffer.MarkAsUsed();
									obj.data = buffer;
								}
								ch.AddCreatedObject(obj);
							}

							// Inform the channel
							var writer = BeginSend(Packet.ResponseCreateAddressableObject);
							writer.Write(playerID);
							writer.Write(channelID);
							writer.Write(uniqueID);
							writer.Write(buffer.buffer, buffer.position, buffer.size);
							EndSend(ch, null, true);
						}
					}
					break;

			}
        }


		
    }



	
}