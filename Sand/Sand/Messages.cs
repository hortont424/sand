using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerState,
        UpdatePlayerClass
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

        // UpdatePlayerState

        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerState, id);

            Storage.packetWriter.Write(player.Position);
            Storage.packetWriter.Write((double)player.Angle);
            Storage.packetWriter.Write((Byte)player.Team);
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.Position = Storage.packetReader.ReadVector2();
            player.Angle = (float)Storage.packetReader.ReadDouble();
            player.Team = (Team)Storage.packetReader.ReadByte();
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.packetReader.ReadVector2();
            Storage.packetReader.ReadDouble();
            Storage.packetReader.ReadByte();
        }

        // UpdatePlayerClass

        public static void SendUpdatePlayerClassMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerClass, id);

            Storage.packetWriter.Write((Byte)player.Class);

            if(immediate)
            {
                if(!Storage.networkSession.IsHost)
                {
                    (player.Gamer as LocalNetworkGamer).SendData(Storage.packetWriter, SendDataOptions.InOrder, Storage.networkSession.Host);
                }
                else
                {
                    var server = (LocalNetworkGamer)Storage.networkSession.Host;

                    server.SendData(Storage.packetWriter, SendDataOptions.InOrder);
                }
            }
        }

        private static void ProcessUpdatePlayerClassMessage(Player player)
        {
            player.Class = (Class)Storage.packetReader.ReadByte();
        }

        private static void DiscardUpdatePlayerClassMessage()
        {
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
                    Console.WriteLine("Skipping a byte!");
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
                        switch (type)
                        {
                            case MessageTypes.UpdatePlayerState:
                                DiscardUpdatePlayerStateMessage();
                                break;
                            case MessageTypes.UpdatePlayerClass:
                                DiscardUpdatePlayerClassMessage();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        continue;
                    }

                    var player = remoteGamer.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        case MessageTypes.UpdatePlayerClass:
                            ProcessUpdatePlayerClassMessage(player);
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
                        case MessageTypes.UpdatePlayerClass:
                            ProcessUpdatePlayerClassMessage(player);

                            foreach(NetworkGamer clientGamer in Storage.networkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerClassMessage(gamer.Tag as Player, gamerId, false);
                            }

                            var server = (LocalNetworkGamer)Storage.networkSession.Host;
                            server.SendData(Storage.packetWriter, SendDataOptions.InOrder);

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
                var player = gamer.Tag as Player;

                if(!Storage.networkSession.IsHost)
                {
                    SendUpdatePlayerStateMessage(player, gamer.Id);

                    /*while(player.Messages.Count != 0)
                    {
                        Message message = player.Messages.Dequeue();

                        switch(message.Type)
                        {
                            case MessageTypes.UpdatePlayerClass:
                                SendUpdatePlayerClassMessage(player, gamer.Id);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }*/

                    gamer.SendData(Storage.packetWriter, SendDataOptions.InOrder, Storage.networkSession.Host);
                }
            }

            if(Storage.networkSession.IsHost)
            {
                foreach(var gamer in Storage.networkSession.AllGamers)
                {
                    var player = gamer.Tag as Player;

                    if(player == null)
                    {
                        continue;
                    }

                    SendUpdatePlayerStateMessage(player, gamer.Id);
                    /*
                    // TODO: not sure about this

                    while(player.Messages.Count != 0)
                    {
                        Message message = player.Messages.Dequeue();

                        switch (message.Type)
                        {
                            case MessageTypes.UpdatePlayerClass:
                                SendUpdatePlayerClassMessage(player, gamer.Id);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }*/
                }

                var server = Storage.networkSession.Host as LocalNetworkGamer;

                if(server != null)
                {
                    server.SendData(Storage.packetWriter, SendDataOptions.InOrder);
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