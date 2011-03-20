using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerPosition,
        UpdatePlayerTeam
    };

    internal class Messages
    {
        // UpdatePlayerPosition

        public static void SendUpdatePlayerPositionMessage(Player player, byte id)
        {
            Storage.packetWriter.Write((int)MessageTypes.UpdatePlayerPosition);
            Storage.packetWriter.Write(id);
            Storage.packetWriter.Write(player.Position);
            Storage.packetWriter.Write((double)player.Angle);
        }

        private static void ProcessUpdatePlayerPositionMessage(Player player)
        {
            player.Position = Storage.packetReader.ReadVector2();
            player.Angle = (float)Storage.packetReader.ReadDouble();
        }

        // UpdatePlayerTeam

        public static void SendUpdatePlayerTeamMessage(Player player, byte id)
        {
            Storage.packetWriter.Write((int)MessageTypes.UpdatePlayerTeam);
            Storage.packetWriter.Write(id);
            Storage.packetWriter.Write((Int32)player.Team);
        }

        private static void ProcessUpdatePlayerTeamMessage(Player player, bool rebroadcast)
        {
            if(player is RemotePlayer)
            {
                if(rebroadcast)
                {
                    player.Team = (Team)Storage.packetReader.ReadInt32();
                }
                else
                {
                    player._team = (Team)Storage.packetReader.ReadInt32();
                }
            }
        }

        private static void UpdateClientStateFromServer(LocalNetworkGamer gamer)
        {
            while(gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                gamer.ReceiveData(Storage.packetReader, out sender);

                while(Storage.packetReader.Position < Storage.packetReader.Length)
                {
                    var type = (MessageTypes)Storage.packetReader.ReadInt32();
                    byte gamerId = Storage.packetReader.ReadByte();
                    NetworkGamer remoteGamer = Storage.networkSession.FindGamerById(gamerId);

                    if(remoteGamer == null)
                    {
                        break;
                    }

                    if(remoteGamer.IsLocal)
                    {
                        continue;
                    }

                    var player = remoteGamer.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerPosition:
                            ProcessUpdatePlayerPositionMessage(player);
                            break;
                        case MessageTypes.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player, false);
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
                    var type = (MessageTypes)Storage.packetReader.ReadInt32();
                    byte gamerId = Storage.packetReader.ReadByte();

                    var player = sender.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerPosition:
                            ProcessUpdatePlayerPositionMessage(player);
                            break;
                        case MessageTypes.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public static void Update()
        {
            foreach(LocalNetworkGamer gamer in Storage.networkSession.LocalGamers)
            {
                if(!Storage.networkSession.IsHost)
                {
                    var player = gamer.Tag as Player;

                    SendUpdatePlayerPositionMessage(player, gamer.Id);

                    while(player.Messages.Count > 0)
                    {
                        MessageTypes messageType = player.Messages.Dequeue();

                        switch(messageType)
                        {
                            case MessageTypes.UpdatePlayerPosition:
                                // send position maybe
                                break;
                            case MessageTypes.UpdatePlayerTeam:
                                SendUpdatePlayerTeamMessage(player, gamer.Id);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

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
                        return;
                    }

                    SendUpdatePlayerPositionMessage(player, gamer.Id);

                    while (player.Messages.Count > 0)
                    {
                        MessageTypes messageType = player.Messages.Dequeue();

                        switch (messageType)
                        {
                            case MessageTypes.UpdatePlayerPosition:
                                // send position maybe
                                break;
                            case MessageTypes.UpdatePlayerTeam:
                                SendUpdatePlayerTeamMessage(player, gamer.Id);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
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