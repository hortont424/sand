using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerState
    } ;

    internal class Messages
    {
        public static void SendMessageHeader(MessageTypes type, byte id)
        {
            Storage.packetWriter.Write((byte)42);
            Storage.packetWriter.Write((byte)24);
            Storage.packetWriter.Write((int)type);
            Storage.packetWriter.Write(id);
        }

        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerState, id);

            Storage.packetWriter.Write(player.Position);
            Storage.packetWriter.Write((double)player.Angle);
            Storage.packetWriter.Write((Byte)player.Team);
            Storage.packetWriter.Write((Byte)player.Class);
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.Position = Storage.packetReader.ReadVector2();
            player.Angle = (float)Storage.packetReader.ReadDouble();
            player.Team = (Team)Storage.packetReader.ReadByte();
            player.Class = (Class)Storage.packetReader.ReadByte();
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.packetReader.ReadVector2();
            Storage.packetReader.ReadDouble();
            Storage.packetReader.ReadByte();
            Storage.packetReader.ReadByte();
        }

        private static bool NextPacketIsValid()
        {
            bool goodPacket = true;
            var magic = Storage.packetReader.ReadBytes(2);

            if(magic.Length != 2 || magic[0] != 42 || magic[1] != 24)
            {
                goodPacket = false;
            }

            Storage.packetReader.Position -= magic.Length;

            return goodPacket;
        }

        private static bool FindNextValidPacket()
        {
            while(Storage.packetReader.Position < Storage.packetReader.Length)
            {
                if(!NextPacketIsValid())
                {
                    Storage.packetReader.ReadByte();
                }
                else
                {
                    Storage.packetReader.Position += 2;

                    return true;
                }
            }

            return false;
        }

        private static void UpdateClientStateFromServer(LocalNetworkGamer gamer)
        {
            while(gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                gamer.ReceiveData(Storage.packetReader, out sender);

                while(Storage.packetReader.Position < Storage.packetReader.Length)
                {
                    if(!FindNextValidPacket())
                    {
                        continue;
                    }

                    var type = (MessageTypes)Storage.packetReader.ReadInt32();
                    var gamerId = Storage.packetReader.ReadByte();
                    var remoteGamer = Storage.networkSession.FindGamerById(gamerId);

                    if(remoteGamer == null || remoteGamer.IsLocal)
                    {
                        DiscardUpdatePlayerStateMessage();
                        continue;
                    }

                    var player = remoteGamer.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static void UpdateServerStateFromClients(LocalNetworkGamer gamer)
        {
            while(gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                gamer.ReceiveData(Storage.packetReader, out sender);

                if(sender.IsLocal)
                {
                    continue;
                }

                while(Storage.packetReader.Position < Storage.packetReader.Length)
                {
                    if(!FindNextValidPacket())
                    {
                        continue;
                    }

                    var type = (MessageTypes)Storage.packetReader.ReadInt32();
                    byte gamerId = Storage.packetReader.ReadByte();

                    var player = sender.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public static void Update()
        {
            foreach(var gamer in Storage.networkSession.LocalGamers)
            {
                if(!Storage.networkSession.IsHost)
                {
                    var player = gamer.Tag as Player;

                    SendUpdatePlayerStateMessage(player, gamer.Id);

                    gamer.SendData(Storage.packetWriter, SendDataOptions.ReliableInOrder, Storage.networkSession.Host);
                }
            }

            if(Storage.networkSession.IsHost)
            {
                foreach(var gamer in Storage.networkSession.AllGamers)
                {
                    var player = gamer.Tag as Player;

                    if(player == null)
                    {
                        return;
                    }

                    SendUpdatePlayerStateMessage(player, gamer.Id);
                }

                var server = Storage.networkSession.Host as LocalNetworkGamer;

                if(server != null)
                {
                    server.SendData(Storage.packetWriter, SendDataOptions.ReliableInOrder);
                }
            }

            Storage.networkSession.Update();

            foreach(LocalNetworkGamer gamer in Storage.networkSession.LocalGamers)
            {
                if(gamer.IsHost)
                {
                    UpdateServerStateFromClients(gamer);
                }
                else
                {
                    UpdateClientStateFromServer(gamer);
                }
            }
        }
    }
}