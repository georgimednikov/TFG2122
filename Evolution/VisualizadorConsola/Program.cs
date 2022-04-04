using EvolutionSimulation;
using System;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Simulation s = new Simulation();
#if DEBUG
            s.Init(10, 10, 10, "../../ProgramData/", "../../ResultingData/");
#else
            if (!UserInfo.AskInfoUsingConsole())
                return;
#endif
            s.Run();
        }
    }
}