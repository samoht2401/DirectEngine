using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectEngine;

namespace Testing
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            TestGame game = new TestGame();
            game.Run();
            game.Dispose();
        }
    }
}
