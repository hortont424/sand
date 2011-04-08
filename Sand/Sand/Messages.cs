using System;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum MessageType
    {
        UpdatePlayerState,
        UpdatePlayerMenuState,
        UpdatePlayerClass,
        UpdatePlayerTeam,
        InvisiblePlayer,
        PlaySound,
        Stun
    } ;

    internal class Messages
    {
        #region Message Helpers

        public static void SendMessageHeader(MessageType type, byte id)
        {
            Storage.PacketWriter.Write((byte)42);
            Storage.PacketWriter.Write((byte)24);
            Storage.PacketWriter.Write((int)type);
            Storage.PacketWriter.Write(id);
        }

        private static void SendOneOffMessage(Player player)
        {
            if(!Storage.NetworkSession.IsHost)
            {
                var gamer = player.Gamer as LocalNetworkGamer;

                if(gamer != null)
                {
                    gamer.SendData(Storage.PacketWriter, SendDataOptions.Reliable, Storage.NetworkSession.Host);
                }
            }
            else
            {
                var server = Storage.NetworkSession.Host as LocalNetworkGamer;

                if(server != null)
                {
                    server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);
                }
            }
        }

        #endregion

        #region UpdatePlayerState Message

        public static void SendUpdatePlayerStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageType.UpdatePlayerState, id);

            Storage.PacketWriter.Write(player.X);
            Storage.PacketWriter.Write(player.Y);
            Storage.PacketWriter.Write(player.Angle);
            Storage.PacketWriter.Write(player.Stunned);

            if(player.Stunned)
            {
                Storage.PacketWriter.Write((UInt64)player.UnstunTime.Ticks);
            }
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.X = Storage.PacketReader.ReadSingle();
            player.Y = Storage.PacketReader.ReadSingle();
            player.Angle = Storage.PacketReader.ReadSingle();
            player.Stunned = Storage.PacketReader.ReadBoolean();

            if(player.Stunned)
            {
                player.UnstunTime = new TimeSpan((long)Storage.PacketReader.ReadUInt64());
            }
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.PacketReader.ReadSingle();
            Storage.PacketReader.ReadSingle();
            Storage.PacketReader.ReadSingle();
            var stunned = Storage.PacketReader.ReadBoolean();

            if(stunned)
            {
                Storage.PacketReader.ReadUInt64();
            }
        }

        #endregion

        #region UpdatePlayerMenuState Message

        public static void SendUpdatePlayerMenuStateMessage(Player player, byte id)
        {
            SendMessageHeader(MessageType.UpdatePlayerMenuState, id);

            Storage.PacketWriter.Write((Byte)player.Class);
            Storage.PacketWriter.Write((Byte)player.Team);
        }

        private static void ProcessUpdatePlayerMenuStateMessage(Player player)
        {
            player.Class = (Class)Storage.PacketReader.ReadByte();
            player.Team = (Team)Storage.PacketReader.ReadByte();
        }

        private static void DiscardUpdatePlayerMenuStateMessage()
        {
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadByte();
        }

        #endregion

        #region UpdatePlayerClass Message

        public static void SendUpdatePlayerClassMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.UpdatePlayerClass, id);

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
            SendMessageHeader(MessageType.UpdatePlayerTeam, id);

            Storage.PacketWriter.Write((Byte)player.Team);

            if(immediate)
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

        #region InvisiblePlayer Message

        public static void SendInvisiblePlayerMessage(Player player, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.InvisiblePlayer, id);

            Storage.PacketWriter.Write(player.Invisible);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static void ProcessInvisiblePlayerMessage(Player player)
        {
            player.Invisible = Storage.PacketReader.ReadBoolean();
        }

        private static void DiscardInvisiblePlayerMessage()
        {
            Storage.PacketReader.ReadBoolean();
        }

        #endregion

        #region PlaySound Message

        public static void SendPlaySoundMessage(Player player, string soundName, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.PlaySound, id);

            Storage.PacketWriter.Write(soundName);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static string ProcessPlaySoundMessage(Player player)
        {
            var soundName = Storage.PacketReader.ReadString();

            Storage.Sound(soundName).Play();
            
            return soundName;
        }

        private static void DiscardPlaySoundMessage()
        {
            Storage.PacketReader.ReadString();
        }

        #endregion

        #region Stun Message

        public static void SendStunMessage(Player player, Player stunnedPlayer, Int32 stunEnergy, byte id,
                                           bool immediate)
        {
            SendMessageHeader(MessageType.Stun, id);

            Storage.PacketWriter.Write(stunnedPlayer.Gamer.Id);
            Storage.PacketWriter.Write(stunEnergy);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static Tuple<byte, int> ProcessStunMessage(Player player)
        {
            var stunId = Storage.PacketReader.ReadByte();
            var stunEnergy = Storage.PacketReader.ReadInt32();

            var localGamer = Storage.NetworkSession.LocalGamers[0];

            if(stunId == localGamer.Id)
            {
                var localPlayer = localGamer.Tag as Player;

                if(localPlayer != null)
                {
                    localPlayer.Stun(stunEnergy);
                }

                return null;
            }

            return Tuple.Create(stunId, stunEnergy);
        }

        private static void DiscardStunMessage()
        {
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadInt32();
        }

        #endregion

        private static bool NextPacketIsValid()
        {
            var goodPacket = true;
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

                    var type = (MessageType)Storage.PacketReader.ReadInt32();
                    var gamerId = Storage.PacketReader.ReadByte();
                    var remoteGamer = Storage.NetworkSession.FindGamerById(gamerId);

                    if(remoteGamer == null || remoteGamer.IsLocal)
                    {
                        switch(type)
                        {
                            case MessageType.UpdatePlayerState:
                                DiscardUpdatePlayerStateMessage();
                                break;
                            case MessageType.UpdatePlayerMenuState:
                                DiscardUpdatePlayerMenuStateMessage();
                                break;
                            case MessageType.UpdatePlayerClass:
                                DiscardUpdatePlayerClassMessage();
                                break;
                            case MessageType.UpdatePlayerTeam:
                                DiscardUpdatePlayerTeamMessage();
                                break;
                            case MessageType.InvisiblePlayer:
                                DiscardInvisiblePlayerMessage();
                                break;
                            case MessageType.PlaySound:
                                DiscardPlaySoundMessage();
                                break;
                            case MessageType.Stun:
                                DiscardStunMessage();
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        continue;
                    }

                    var player = remoteGamer.Tag as Player;

                    switch(type)
                    {
                        case MessageType.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        case MessageType.UpdatePlayerMenuState:
                            ProcessUpdatePlayerMenuStateMessage(player);
                            break;
                        case MessageType.UpdatePlayerClass:
                            ProcessUpdatePlayerClassMessage(player);
                            break;
                        case MessageType.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);
                            break;
                        case MessageType.InvisiblePlayer:
                            ProcessInvisiblePlayerMessage(player);
                            break;
                        case MessageType.PlaySound:
                            ProcessPlaySoundMessage(player);
                            break;
                        case MessageType.Stun:
                            ProcessStunMessage(player);
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

                    var type = (MessageType)Storage.PacketReader.ReadInt32();
                    byte gamerId = Storage.PacketReader.ReadByte();

                    var player = sender.Tag as Player;

                    LocalNetworkGamer server;

                    switch(type)
                    {
                        case MessageType.UpdatePlayerState:
                            ProcessUpdatePlayerStateMessage(player);
                            break;
                        case MessageType.UpdatePlayerMenuState:
                            ProcessUpdatePlayerMenuStateMessage(player);
                            break;
                        case MessageType.UpdatePlayerClass:
                            ProcessUpdatePlayerClassMessage(player);

                            foreach(var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerClassMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);

                            foreach(var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendUpdatePlayerTeamMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.InvisiblePlayer:
                            ProcessInvisiblePlayerMessage(player);

                            foreach(var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendInvisiblePlayerMessage(gamer.Tag as Player, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.PlaySound:
                            var soundName = ProcessPlaySoundMessage(player);

                            foreach(var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                SendPlaySoundMessage(gamer.Tag as Player, soundName, gamerId, false);
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.Stun:
                            var stunInfo = ProcessStunMessage(player);

                            if(stunInfo == null)
                            {
                                break;
                            }

                            var stunId = stunInfo.Item1;
                            var stunEnergy = stunInfo.Item2;

                            foreach(var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                if(clientGamer.Id == stunId)
                                {
                                    SendStunMessage(gamer.Tag as Player, clientPlayer, stunEnergy, gamerId, false);
                                }
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
                    if(Storage.NetworkSession.IsEveryoneReady)
                    {
                        SendUpdatePlayerStateMessage(player, gamer.Id);
                    }
                    else
                    {
                        SendUpdatePlayerMenuStateMessage(player, gamer.Id);
                    }

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

                    if(Storage.NetworkSession.IsEveryoneReady)
                    {
                        SendUpdatePlayerStateMessage(player, gamer.Id);
                    }
                    else
                    {
                        SendUpdatePlayerMenuStateMessage(player, gamer.Id);
                    }
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