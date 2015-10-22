using System;

namespace FruitNinja
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (NinjaWindow game = new NinjaWindow())
            {
                game.Run();
            }
        }
    }
#endif
}

