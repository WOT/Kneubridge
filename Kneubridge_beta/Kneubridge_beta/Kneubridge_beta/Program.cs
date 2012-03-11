using System;

namespace Kneubridge_beta
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KinectGame game = new KinectGame())
            {
                game.Run();
            }
        }
    }
#endif
}

