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
            Console.WriteLine(Math.Atan2(0, 1) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(1, 1) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(1, 0) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(1, -1) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(0, -1) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(-1, -1) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(-1, 0) * (180 / Math.PI));
            Console.WriteLine(Math.Atan2(-1, 1) * (180 / Math.PI));
#else
            if (!UserInfo.AskInfoUsingConsole())
                return;
#endif
            //EvolutionSimulation.Genetics.CreatureChromosome.SetChromosome();
            //ISimulation s = new ConsoleSimulation();
            //s.Init();
            //s.Run();
        }
    }
}