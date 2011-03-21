using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerPosition
    };

    internal class Messages
    {
        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            Storage.packetWriter.Write((int)MessageTypes.UpdatePlayerPosition);
            Storage.packetWriter.Write(id);
            Storage.packetWriter.Write(player.Position);
            Storage.packetWriter.Write((double)player.Angle);
            Storage.packetWriter.Write((Byte)player.Team);
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.Position = Storage.packetReader.ReadVector2();
            player.Angle = (float)Storage.packetReader.ReadDouble();
            player.Team = (Team)Storage.packetReader.ReadByte();

            Console.WriteLine("Got team {0} for {1}", player.Team, player.ToString());
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
                    var type = (MessageTypes)Storage.packetReader.ReadInt32();
                    byte gamerId = Storage.packetReader.ReadByte();

                    var player = sender.Tag as Player;

                    switch(type)
                    {
                        case MessageTypes.UpdatePlayerPosition:
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