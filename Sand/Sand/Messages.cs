﻿using System;
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
        #region Message Helpers

        public static void SendMessageHeader(MessageTypes type, byte id)
        {
            Storage.PacketWriter.Write((byte)42);
            Storage.PacketWriter.Write((byte)24);
            Storage.PacketWriter.Write((int)type);
            Storage.PacketWriter.Write(id);
        }

        private static void SendOneOffMessage(Player player)
        {
            if (!Storage.NetworkSession.IsHost)
            {
                var gamer = player.Gamer as LocalNetworkGamer;

                if (gamer != null)
                {
                    gamer.SendData(Storage.PacketWriter, SendDataOptions.Reliable, Storage.NetworkSession.Host);
                }
            }
            else
            {
                var server = Storage.NetworkSession.Host as LocalNetworkGamer;

                if (server != null)
                {
                    server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);
                }
            }
        }

        #endregion

        #region UpdatePlayerState Message

        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerState, id);

            Storage.PacketWriter.Write(player.Position);
            Storage.PacketWriter.Write((double)player.Angle);
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.Position = Storage.PacketReader.ReadVector2();
            player.Angle = (float)Storage.PacketReader.ReadDouble();
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.PacketReader.ReadVector2();
            Storage.PacketReader.ReadDouble();
        }

        #endregion

        #region UpdatePlayerClass Message

        public static void SendUpdatePlayerClassMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerClass, id);

            Storage.PacketWriter.Write((Byte)player.Class);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static void ProcessUpdatePlayerClassMessage(Player player)
        {
            player.Class = (Class)Storage.PacketReader.ReadByte();
        }

        private static void DiscardUpdatePlayerClassMessage()
        {
            Storage.PacketReader.ReadByte();
        }

        #endregion

        #region UpdatePlayerTeam Message

        public static void SendUpdatePlayerTeamMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageTypes.UpdatePlayerTeam, id);

            Storage.PacketWriter.Write((Byte)player.Team);

            if (immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static void ProcessUpdatePlayerTeamMessage(Player player)
        {
            player.Team = (Team)Storage.PacketReader.ReadByte();
        }

        private static void DiscardUpdatePlayerTeamMessage()
        {
            Storage.PacketReader.ReadByte();
        }

        #endregion

        private static bool NextPacketIsValid()
        {
            bool goodPacket = true;
            var magic = Storage.PacketReader.ReadBytes(2);

            if(magic.Length != 2 || magic[0] != 42 || magic[1] != 24)
            {
                goodPacket = false;
            }

            Storage.PacketReader.Position -= magic.Length;

            return goodPacket;
        }

        private static bool FindNextValidPacket()
        {
            while(Storage.PacketReader.Position < Storage.PacketReader.Length)
            {
                if(!NextPacketIsValid())
                {
                    Console.WriteLine("Skipping a byte!");
                    Storage.PacketReader.ReadByte();
                }
                else
                {
                    Storage.PacketReader.Position += 2;

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
                gamer.ReceiveData(Storage.PacketReader, out sender);

                while(Storage.PacketReader.Position < Storage.PacketReader.Length)
                {
                    if(!FindNextValidPacket())
                    {
                        continue;
                    }

                    var type = (MessageTypes)Storage.PacketReader.ReadInt32();
                    var gamerId = Storage.PacketReader.ReadByte();
                    var remoteGamer = Storage.NetworkSession.FindGamerById(gamerId);

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
                gamer.ReceiveData(Storage.PacketReader, out sender);

                if(sender.IsLocal)
                {
                    continue;
                }

                while(Storage.PacketReader.Position < Storage.PacketReader.Length)
                {
                    if(!FindNextValidPacket())
                    {
                        continue;
                    }

                    var type = (MessageTypes)Storage.PacketReader.ReadInt32();
                    byte gamerId = Storage.PacketReader.ReadByte();

                    var player = sender.Tag as Player;

                    LocalNetworkGamer server;
                    
                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        case MessageTypes.UpdatePlayerClass:
                            ProcessUpdatePlayerClassMessage(player);

                            foreach(NetworkGamer clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerClassMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageTypes.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);

                            foreach (NetworkGamer clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerTeamMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public static void Update()
        {
            foreach(var gamer in Storage.NetworkSession.LocalGamers)
            {
                var player = gamer.Tag as Player;

                if(!Storage.NetworkSession.IsHost)
                {
                    SendUpdatePlayerStateMessage(player, gamer.Id);

                    gamer.SendData(Storage.PacketWriter, SendDataOptions.InOrder, Storage.NetworkSession.Host);
                }
            }

            if(Storage.NetworkSession.IsHost)
            {
                foreach(var gamer in Storage.NetworkSession.AllGamers)
                {
                    var player = gamer.Tag as Player;

                    if(player == null)
                    {
                        continue;
                    }

                    SendUpdatePlayerStateMessage(player, gamer.Id);
                }

                var server = Storage.NetworkSession.Host as LocalNetworkGamer;

                if(server != null)
                {
                    server.SendData(Storage.PacketWriter, SendDataOptions.InOrder);
                }
            }

            Storage.NetworkSession.Update();

            foreach(LocalNetworkGamer gamer in Storage.NetworkSession.LocalGamers)
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