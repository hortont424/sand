namespace Sand
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using(var game = new Sand())
            {
                game.Run();
            }
        }
    }
}