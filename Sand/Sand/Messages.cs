using System;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;

namespace Sand
{
    public enum MessageType
    {
        UpdatePlayerState,
        UpdatePlayerMenuState,
        UpdatePlayerClass,
        UpdatePlayerTeam,
        PlaySound,
        Stun,
        ActivateTool,
        CreateSand,
        RemoveSand,
        UpdateSand
    }

    internal class ActivationInfo
    {
        public ToolSlot Slot;
        public bool State;
        public string PropertyName;
        public float PropertyValue;

        public ActivationInfo(ToolSlot slot, bool state, string propertyName, float propertyValue)
        {
            Slot = slot;
            State = state;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
    }

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

        public static void SendOneOffMessage(Player player)
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
            Storage.PacketWriter.Write(player.Invisible);
            Storage.PacketWriter.Write(player.PureAcceleration);

            if(player.Stunned)
            {
                Storage.PacketWriter.Write((UInt64)player.StunTimeRemaining.Ticks);
            }
        }

        private static void ProcessUpdatePlayerStateMessage(Player player)
        {
            player.X = Storage.PacketReader.ReadSingle();
            player.Y = Storage.PacketReader.ReadSingle();
            player.Angle = Storage.PacketReader.ReadSingle();
            player.Stunned = Storage.PacketReader.ReadBoolean();
            player.Invisible = Storage.PacketReader.ReadSingle();
            player.PureAcceleration = Storage.PacketReader.ReadVector2();

            if(player.Stunned)
            {
                player.StunTimeRemaining = new TimeSpan((long)Storage.PacketReader.ReadUInt64());
            }
        }

        private static void DiscardUpdatePlayerStateMessage()
        {
            Storage.PacketReader.ReadSingle();
            Storage.PacketReader.ReadSingle();
            Storage.PacketReader.ReadSingle();
            var stunned = Storage.PacketReader.ReadBoolean();
            Storage.PacketReader.ReadSingle();
            Storage.PacketReader.ReadVector2();

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

            Storage.PacketWriter.Write((byte)(player.PrimaryA != null ? player.PrimaryA.Type : ToolType.None));
            Storage.PacketWriter.Write((byte)(player.PrimaryB != null ? player.PrimaryB.Type : ToolType.None));
            Storage.PacketWriter.Write((byte)(player.Weapon != null ? player.Weapon.Type : ToolType.None));
            Storage.PacketWriter.Write((byte)(player.Mobility != null ? player.Mobility.Type : ToolType.None));
            Storage.PacketWriter.Write((byte)(player.Utility != null ? player.Utility.Type : ToolType.None));
        }

        private static void ProcessUpdatePlayerMenuStateMessage(Player player)
        {
            player.Class = (Class)Storage.PacketReader.ReadByte();
            player.Team = (Team)Storage.PacketReader.ReadByte();

            var primaryAType = (ToolType)Storage.PacketReader.ReadByte();
            var primaryBType = (ToolType)Storage.PacketReader.ReadByte();
            var weaponType = (ToolType)Storage.PacketReader.ReadByte();
            var mobilityType = (ToolType)Storage.PacketReader.ReadByte();
            var utilityType = (ToolType)Storage.PacketReader.ReadByte();

            // TODO: copypasta

            if(primaryAType != ToolType.None && (player.PrimaryA == null || primaryAType != player.PrimaryA.Type))
            {
                player.PrimaryA = Tool.OfType(primaryAType, player);
            }

            if(primaryBType != ToolType.None && (player.PrimaryB == null || primaryBType != player.PrimaryB.Type))
            {
                player.PrimaryB = Tool.OfType(primaryBType, player);
            }

            if(weaponType != ToolType.None && (player.Weapon == null || weaponType != player.Weapon.Type))
            {
                player.Weapon = Tool.OfType(weaponType, player);
            }

            if(mobilityType != ToolType.None && (player.Mobility == null || mobilityType != player.Mobility.Type))
            {
                player.Mobility = Tool.OfType(mobilityType, player);
            }

            if(utilityType != ToolType.None && (player.Utility == null || utilityType != player.Utility.Type))
            {
                player.Utility = Tool.OfType(utilityType, player);
            }
        }

        private static void DiscardUpdatePlayerMenuStateMessage()
        {
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadByte();

            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadByte();
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

        #region ActivateTool Message

        public static void SendActivateToolMessage(Player player, ToolSlot slot, bool newState, string propertyName,
                                                   float propertyValue, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.ActivateTool, id);

            Storage.PacketWriter.Write((byte)slot);
            Storage.PacketWriter.Write(newState);
            Storage.PacketWriter.Write(propertyName ?? "");
            Storage.PacketWriter.Write(propertyValue);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static ActivationInfo ProcessActivateToolMessage(Player player)
        {
            var slot = (ToolSlot)Storage.PacketReader.ReadByte();
            var state = Storage.PacketReader.ReadBoolean();

            // This is scary.

            var propertyName = Storage.PacketReader.ReadString();
            var propertyValue = Storage.PacketReader.ReadSingle();

            if(player is RemotePlayer)
            {
                dynamic tool = player.ToolInSlot(slot);
                tool.Active = state;

                if(propertyName != "")
                {
                    var toolType = tool.GetType();
                    var property = toolType.GetProperty(propertyName);

                    if(property != null)
                    {
                        var propertyType = property.PropertyType;

                        property.SetValue(tool,
                                          Convert.ChangeType(propertyValue, propertyType),
                                          null);
                    }
                }
            }

            return new ActivationInfo(slot, state, propertyName, propertyValue);
        }

        private static void DiscardActivateToolMessage()
        {
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadBoolean();
            Storage.PacketReader.ReadString();
            Storage.PacketReader.ReadSingle();
        }

        #endregion

        #region CreateSand Message

        public static void SendCreateSandMessage(Player player, Particle p, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.CreateSand, id);

            Storage.PacketWriter.Write(p.Id);
            Storage.PacketWriter.Write(p.Owner);
            Storage.PacketWriter.Write(p.Position);
            Storage.PacketWriter.Write(p.Velocity);
            Storage.PacketWriter.Write((byte)p.Team);

            Storage.PacketWriter.Write(p.Alive);
            Storage.PacketWriter.Write(p.OnFire);

            if(p.OnFire)
            {
                Storage.PacketWriter.Write(p.Fire);
            }

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static Particle ProcessCreateSandMessage(Player player)
        {
            var id = Storage.PacketReader.ReadString();
            var exists = Storage.SandParticles.Particles.ContainsKey(id);

            var p = exists
                        ? Storage.SandParticles.Particles[id]
                        : new Particle(id);

            p.Owner = Storage.PacketReader.ReadByte();
            p.Position = Storage.PacketReader.ReadVector2();
            p.Velocity = Storage.PacketReader.ReadVector2();
            p.Team = (Team)Storage.PacketReader.ReadByte();
            p.Alive = Storage.PacketReader.ReadBoolean();
            p.OnFire = Storage.PacketReader.ReadBoolean();

            if(p.OnFire)
            {
                p.Fire = Storage.PacketReader.ReadByte();
            }

            if(!exists)
            {
                Storage.SandParticles.Emit(p, false);
            }

            return p;
        }

        private static void DiscardCreateSandMessage()
        {
            Storage.PacketReader.ReadString();
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadVector2();
            Storage.PacketReader.ReadVector2();
            Storage.PacketReader.ReadByte();
            Storage.PacketReader.ReadBoolean();
            var onFire = Storage.PacketReader.ReadBoolean();

            if(onFire)
            {
                Storage.PacketReader.ReadByte();
            }
        }

        #endregion

        #region UpdateSand Message

        public static void SendUpdateSandMessage(Player player, Particle p, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.UpdateSand, id);

            Storage.PacketWriter.Write(p.Id);
            Storage.PacketWriter.Write(p.Position);
            Storage.PacketWriter.Write(p.Velocity);

            Storage.PacketWriter.Write(p.Alive);
            Storage.PacketWriter.Write(p.OnFire);

            if (p.OnFire)
            {
                Storage.PacketWriter.Write(p.Fire);
            }

            if (immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static Particle ProcessUpdateSandMessage(Player player)
        {
            var id = Storage.PacketReader.ReadString();
            var exists = Storage.SandParticles.Particles.ContainsKey(id);

            var p = exists
                        ? Storage.SandParticles.Particles[id]
                        : new Particle(id);

            p.Position = Storage.PacketReader.ReadVector2();
            p.Velocity = Storage.PacketReader.ReadVector2();
            p.Alive = Storage.PacketReader.ReadBoolean();
            p.OnFire = Storage.PacketReader.ReadBoolean();

            if (p.OnFire)
            {
                p.Fire = Storage.PacketReader.ReadByte();
            }

            return p;
        }

        private static void DiscardUpdateSandMessage()
        {
            Storage.PacketReader.ReadString();
            Storage.PacketReader.ReadVector2();
            Storage.PacketReader.ReadVector2();
            Storage.PacketReader.ReadBoolean();
            var onFire = Storage.PacketReader.ReadBoolean();

            if (onFire)
            {
                Storage.PacketReader.ReadByte();
            }
        }

        #endregion

        #region RemoveSand Message

        public static void SendRemoveSandMessage(Player player, Particle p, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.RemoveSand, id);

            Storage.PacketWriter.Write(p.Id);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        public static void SendRemoveSandMessage(Player player, string particleId, byte id, bool immediate)
        {
            SendMessageHeader(MessageType.RemoveSand, id);

            Storage.PacketWriter.Write(particleId);

            if(immediate)
            {
                SendOneOffMessage(player);
            }
        }

        private static string ProcessRemoveSandMessage(Player player)
        {
            var id = Storage.PacketReader.ReadString();

            if(Storage.SandParticles.Particles.ContainsKey(id))
            {
                Storage.SandParticles.Particles.Remove(id);
            }

            return id;
        }

        private static void DiscardRemoveSandMessage()
        {
            Storage.PacketReader.ReadString();
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
                            case MessageType.PlaySound:
                                DiscardPlaySoundMessage();
                                break;
                            case MessageType.Stun:
                                DiscardStunMessage();
                                break;
                            case MessageType.ActivateTool:
                                DiscardActivateToolMessage();
                                break;
                            case MessageType.CreateSand:
                                DiscardCreateSandMessage();
                                break;
                            case MessageType.UpdateSand:
                                DiscardUpdateSandMessage();
                                break;
                            case MessageType.RemoveSand:
                                DiscardRemoveSandMessage();
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
                        case MessageType.PlaySound:
                            ProcessPlaySoundMessage(player);
                            break;
                        case MessageType.Stun:
                            ProcessStunMessage(player);
                            break;
                        case MessageType.ActivateTool:
                            ProcessActivateToolMessage(player);
                            break;
                        case MessageType.CreateSand:
                            ProcessCreateSandMessage(player);
                            break;
                        case MessageType.UpdateSand:
                            ProcessUpdateSandMessage(player);
                            break;
                        case MessageType.RemoveSand:
                            ProcessRemoveSandMessage(player);
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
                    Particle particle;

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

                            SendUpdatePlayerClassMessage(gamer.Tag as Player, gamerId, false);

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.UpdatePlayerTeam:
                            ProcessUpdatePlayerTeamMessage(player);

                            SendUpdatePlayerTeamMessage(gamer.Tag as Player, gamerId, false);

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.PlaySound:
                            var soundName = ProcessPlaySoundMessage(player);

                            SendPlaySoundMessage(gamer.Tag as Player, soundName, gamerId, false);

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

                            foreach (var clientGamer in Storage.NetworkSession.AllGamers)
                            {
                                var clientPlayer = clientGamer.Tag as Player;

                                if (clientGamer.Id == stunId)
                                {
                                    SendStunMessage(gamer.Tag as Player, clientPlayer, stunEnergy, gamerId, false);
                                }
                            }

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.ActivateTool:
                            var aInfo = ProcessActivateToolMessage(player);

                            SendActivateToolMessage(gamer.Tag as Player, aInfo.Slot, aInfo.State, aInfo.PropertyName,
                                                    aInfo.PropertyValue, gamerId, false);

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.CreateSand:
                            particle = ProcessCreateSandMessage(player);

                            SendCreateSandMessage(gamer.Tag as Player, particle, gamerId, false);

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.Reliable);

                            break;
                        case MessageType.UpdateSand:
                            particle = ProcessUpdateSandMessage(player);

                            SendUpdateSandMessage(gamer.Tag as Player, particle, gamerId, false);

                            server = (LocalNetworkGamer)Storage.NetworkSession.Host;
                            server.SendData(Storage.PacketWriter, SendDataOptions.None);

                            break;
                        case MessageType.RemoveSand:
                            var id = ProcessRemoveSandMessage(player);

                            SendRemoveSandMessage(gamer.Tag as Player, id, gamerId, false);

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