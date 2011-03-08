using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageTypes
    {
        UpdatePlayerPosition
    } ;

    internal class Messages
    {
        public static void SendUpdatePlayerPositionMessage(Player player, byte id)
        {
            Storage.packetWriter.Write((int) MessageTypes.UpdatePlayerPosition);
            Storage.packetWriter.Write(id);
            Storage.packetWriter.Write(player._position);

            var server = Storage.networkSession.Host as LocalNetworkGamer;

            if(server != null)
            {
                server.SendData(Storage.packetWriter, SendDataOptions.None);
            }
        }

        private static void ProcessUpdatePlayerPositionMessage(Player player)
        {
            player._position = Storage.packetReader.ReadVector2();
        }

        private static void UpdateClientStateFromServer(LocalNetworkGamer gamer)
        {
            while(gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                gamer.ReceiveData(Storage.packetReader, out sender);

                while(Storage.packetReader.Position < Storage.packetReader.Length)
                {
                    var type = (MessageTypes) Storage.packetReader.ReadInt32();
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
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static void UpdateServerStateFromClients(LocalNetworkGamer gamer)
        {
            if(gamer != null && gamer.IsDataAvailable)
            {
            }
        }

        public static void Update()
        {
            foreach(LocalNetworkGamer gamer in Storage.networkSession.LocalGamers)
            {
                // TODO: write out updates from this client to the server!

                if(!Storage.networkSession.IsHost)
                {
                    SendUpdatePlayerPositionMessage(gamer.Tag as Player, gamer.Id);
                }
            }

            if(Storage.networkSession.IsHost)
            {
                // TODO: write updates from server to clients!

                foreach(var gamer in Storage.networkSession.AllGamers)
                {
                    var player = gamer.Tag as Player;

                    if(player == null)
                    {
                        return;
                    }

                    SendUpdatePlayerPositionMessage(player, gamer.Id);
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