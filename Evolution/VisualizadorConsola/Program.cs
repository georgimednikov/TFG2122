using EvolutionSimulation;
using System;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            UserInfo.SetDebugInfo();
#else
            if (!UserInfo.AskInfoUsingConsole())
                return;
#endif
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
        }
    }
}