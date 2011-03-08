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
        public static void SendUpdatePlayerPositionMessage(Player player)
        {
            Storage.packetWriter.Write((int)MessageTypes.UpdatePlayerPosition);
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
                    // TODODODODODODO
                }

                var type = (MessageTypes)Storage.packetReader.ReadInt32();

                switch(type)
                {
                    case MessageTypes.UpdatePlayerPosition:
                        ProcessUpdatePlayerPositionMessage(sender.Tag as Player);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void UpdateServerStateFromClients()
        {
            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                var netGamer = gamer as LocalNetworkGamer;

                if(netGamer != null && netGamer.IsDataAvailable)
                {
                }
            }
        }
    }
}