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
#if true
            //s.Init(10, 10, 10, "../../ProgramData/", "../../ResultingData/");
            s.Init(10, 20, 20, File.ReadAllText("../../ProgramData/UniverseParameters.json"), File.ReadAllText("../../ProgramData/Chromosome.json"),
                 File.ReadAllText("../../ProgramData/AbilityUnlock.json"),  File.ReadAllText("../../ProgramData/SimilarityGeneWeight.json")
                , null /*File.ReadAllText("../../ResultingData/World.json")*/,  null/*File.ReadAllText("../../ResultingData/HighMap.json")*/,  "../../ResultingData/");
#else
            if (!s.AskInfoUsingConsole())
                return;
#endif
            s.Run();
            s.Export();
        }
    }
}