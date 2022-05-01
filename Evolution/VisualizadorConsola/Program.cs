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
            //s.Init(10, 20, 20, "../../ProgramData/", "../../ResultingData/", null);
            s.Init(10, 20, 20, File.ReadAllText("../../ProgramData/UniverseParameters.json"), File.ReadAllText("../../ProgramData/Chromosome.json"),
                 File.ReadAllText("../../ProgramData/AbilityUnlock.json"),  File.ReadAllText("../../ProgramData/SimilarityGeneWeight.json")
                ,  File.ReadAllText("../../ResultingData/World.json"),  File.ReadAllText("../../ResultingData/HeightMap.json"),  "../../ResultingData/", "../../ResultingData/");
#else
            if (!s.AskInfoUsingConsole(s))
                return;
#endif
            s.Run();
            s.Export();
        }
    }
}