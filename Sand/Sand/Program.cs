using System;
using System.Windows.Forms;

namespace Sand
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using(var game = new Sand())
            {
                try
                {
                    game.Run();
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message + e.StackTrace);
                }
            }
        }
    }
}