namespace Sand
{
#if WINDOWS || XBOX
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if(args.Length == 2)
            {
                System.Console.WriteLine(args[1]);
            }

            using(var game = new Sand())
            {
                game.Run();
            }
        }
    }
#endif
}

