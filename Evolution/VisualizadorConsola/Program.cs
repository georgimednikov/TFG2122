using System;
using EvolutionSimulation;
using System.IO;
namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ConsoleSimulation s = new ConsoleSimulation();
#if DEBUG
            s.Init(10, 20, 20, "../../ProgramData/", "../../ResultingData/", null);
#else
            if (!s.AskInfoUsingConsole(s))
                return;
#endif
            s.Run();
            s.Export();
        }
    }
}