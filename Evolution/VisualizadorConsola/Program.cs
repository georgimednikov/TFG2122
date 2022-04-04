using System;
using EvolutionSimulation;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ConsoleSimulation s = new ConsoleSimulation();
#if DEBUG
            s.Init(10, 10, 10, "../../ProgramData/", "../../ResultingData/");
#else
            if (!s.AskInfoUsingConsole())
                return;
#endif
            s.Run();
            s.Export();
        }
    }
}