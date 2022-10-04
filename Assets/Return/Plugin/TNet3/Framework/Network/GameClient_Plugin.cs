using UnityEngine;
using System.IO;
using System.Net;

namespace TNet
{
    public partial class GameClient // extension
    {
		public void ProcessCustomPacket(Packet response, BinaryReader reader, IPEndPoint source)
		{
			Debug.Log("On " + response);

			switch (response)
			{
				case Packet.ResponseCreateAddressableObject:
					{

						//if (onCreate != null)
						{
							packetSourceID = reader.ReadInt32();
							int channelID = reader.ReadInt32();
							uint objID = reader.ReadUInt32();
							TNManager.OnResponseCreateAddressableObject(channelID, packetSourceID, objID, reader);
							packetSourceID = -1;
						}
						break;
					}
			}
		}
	}



	
}