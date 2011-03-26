namespace Sand
{
    internal class Message
    {
        public MessageTypes Type;
        public Player Player;

        public Message(Player player, MessageTypes type)
        {
            Type = type;
            Player = player;
        }
    }
}