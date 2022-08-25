using System;
using ZEDRatApp.ZEDRAT.Networking;

namespace ZEDRatApp.ZEDRAT.Packets.ServerPackets
{
	[Serializable]
	public class GetSystemInfo : IPacket
	{
		public void Execute(Client client)
		{
			client.Send(this);
		}
	}
}