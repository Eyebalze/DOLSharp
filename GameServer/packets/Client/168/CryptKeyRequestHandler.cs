/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CryptKeyRequest, "Handles crypt key requests", eClientStatus.None)]
	public class CryptKeyRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			// for 1.115c+ The First client packet Changes.
			if (client.Version < GameClient.eClientVersion.Version1115)
			{
				int rc4 = packet.ReadByte();
				byte clientType = (byte)packet.ReadByte();
				client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
				client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);
				byte major = (byte)packet.ReadByte();
				byte minor = (byte)packet.ReadByte();
				byte build = (byte)packet.ReadByte();
				if(rc4==1)
				{
					//DOLConsole.Log("SBox=\n");
					//DOLConsole.LogDump(client.PacketProcessor.Encoding.SBox);
					packet.Read(((PacketEncoding168)client.PacketProcessor.Encoding).SBox,0,256);
					((PacketEncoding168)client.PacketProcessor.Encoding).EncryptionState=PacketEncoding168.eEncryptionState.PseudoRC4Encrypted;
					//DOLConsole.WriteLine(client.Socket.RemoteEndPoint.ToString()+": SBox set!");
					//DOLConsole.Log("SBox=\n");
					//DOLConsole.LogDump(((PacketEncoding168)client.PacketProcessor.Encoding).SBox);
				}
				else
				{
				  //Send the crypt key to the client
					client.Out.SendVersionAndCryptKey();
				}
			}
			else
			{
				// we don't handle Encryption for 1.115c
				// the rc4 secret can't be unencrypted from RSA.
				
				// register client type
				byte clientType = (byte)packet.ReadByte();
				client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
				client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);
				
				// if the DataSize is above 7 then the RC4 key is bundled
				// this is stored in case we find a way to handle encryption someday !
				if (packet.DataSize > 7)
				{
					packet.Skip(6);
					ushort length = packet.ReadShortLowEndian();
					packet.Read(client.PacketProcessor.Encoding.SBox, 0, length);
					// ((PacketEncoding168)client.PacketProcessor.Encoding).EncryptionState=PacketEncoding168.eEncryptionState.PseudoRC4Encrypted;
				}
				
				//Send the crypt key to the client
				client.Out.SendVersionAndCryptKey();
			}
		}
	}
}