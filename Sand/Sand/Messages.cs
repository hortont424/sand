using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerState,
        UpdatePlayerClass,
        UpdatePlayerTeam
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

        private static void SendOneOffMessage(Player player)
        {
            if (!Storage.networkSession.IsHost)
            {
                var gamer = player.Gamer as LocalNetworkGamer;

                if (gamer != null)
                {
                    gamer.SendData(Storage.packetWriter, SendDataOptions.Reliable, Storage.networkSession.Host);
                }
            }
            else
            {
                var server = Storage.networkSession.Host as LocalNetworkGamer;

                if (server != null)
                {
                    server.SendData(Storage.packetWriter, SendDataOptions.Reliable);
                }
            }
        }

        // UpdatePlayerState

        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerState, id);

            Storage.packetWriter.Write(player.Position);
            Storage.packetWriter.Write((double)player.Angle);
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.Position = Storage.packetReader.ReadVector2();
            player.Angle = (float)Storage.packetReader.ReadDouble();
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.packetReader.ReadVector2();
            Storage.packetReader.ReadDouble();
        }

        // UpdatePlayerClass

        public static void SendUpdatePlayerClassMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerClass, id);

            Storage.packetWriter.Write((Byte)player.Class);

            if(immediate)
            {
                SendOneOffMessage(player);
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

        // UpdatePlayerTeam

        public static void SendUpdatePlayerTeamMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerTeam, id);

            Storage.packetWriter.Write((Byte)player.Team);

            if (immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static void ProcessUpdatePlayerTeamMessage(Player player)
        {
            player.Team = (Team)Storage.packetReader.ReadByte();
        }

        private static void DiscardUpdatePlayerTeamMessage()
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
                            case MessageTypes.UpdatePlayerTeam:
                                DiscardUpdatePlayerTeamMessage();
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
                        case MessageTypes.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);
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

                    LocalNetworkGamer server;
                    
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

                            server = (LocalNetworkGamer)Storage.networkSession.Host;
                            server.SendData(Storage.packetWriter, SendDataOptions.Reliable);

                            break;
                        case MessageTypes.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);

                            foreach (NetworkGamer clientGamer in Storage.networkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerTeamMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.networkSession.Host;
                            server.SendData(Storage.packetWriter, SendDataOptions.Reliable);

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