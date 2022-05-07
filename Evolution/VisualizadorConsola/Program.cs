using System;
using EvolutionSimulation;
using System.IO;
using Newtonsoft.Json;

namespace VisualizadorConsola
{
    public static class Program
    {
        static ConsoleLoadingBar loadingBar;
        static void Main(string[] args)
        {
            EventSimulation s = new EventSimulation();
            s.OnSimulationBegin += (e) => { loadingBar = new ConsoleLoadingBar(); };
            s.OnSimulationStep += (e) => { loadingBar.Update(e); };
#if true
            s.Init(20, 30, 20, "../../ProgramData/", "../../ResultingData/", null);
#else
            if (!AskInfoUsingConsole(s))
                return;
#endif
            s.Run();
        }

        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses the program's console to do so.
        /// </summary>
        /// <returns></returns>
        static public bool AskInfoUsingConsole(Simulation s)
        {
            string dataDir, exportDir;
            int years, species, individuals;

            do
            {
                Console.WriteLine("Input a valid directory containing the necessary data files for the program (chromosome.json, etc.):\n");
                dataDir = Console.ReadLine() + "\\";
                Console.Clear();
            } while (!Directory.Exists(dataDir));

            Console.WriteLine("Input a valid directory in which the resulting data will be saved:\n");
            exportDir = Console.ReadLine() + "\\";
            Console.Clear();
            if (!Directory.Exists(exportDir))
                exportDir = Directory.CreateDirectory(exportDir).FullName;

            do
            {
                Console.WriteLine("Input how many years of evolution are going to be simulated:");
                string input = Console.ReadLine();
                years = -1;
                if (input != "") years = int.Parse(input);
                Console.Clear();
            } while (years < 0);

            int minSize = UserInfo.MinWorldSize();

            WorldGenConfig config = null;

            // If a simulation world is not given, a new one has to be created.
            if (!File.Exists(dataDir + UserInfo.WorldName) || !File.Exists(dataDir + UserInfo.RegionMapName))
            {
                // If a height map is provided, it is not created from scratch.
                if (File.Exists(dataDir + UserInfo.HeightMapName))
                {
                    string map = File.ReadAllText(dataDir + UserInfo.HeightMapName);
                    float[,] heights = JsonConvert.DeserializeObject<float[,]>(map);
                    config = new WorldGenConfig(World.MapType.Custom)
                    {
                        heightMap = heights,
                        mapSize = heights.GetLength(0)
                    };
                    UserInfo.Size = config.mapSize;
                }
                else // if nothing is given, a size has to be asked of the user. 
                {
                    do
                    {
                        Console.WriteLine("Input how big in squares the world is going to be. Must be a number larger than or equal to: " + minSize + "\n");
                        string input = Console.ReadLine();
                        UserInfo.Size = -1;
                        if (input != "") UserInfo.Size = int.Parse(input);
                        Console.Clear();
                    } while (UserInfo.Size < minSize);
                }
            }

            int minSpecies = UserInfo.MinSpeciesAmount();
            do
            {
                Console.WriteLine("Input how many species are going to be created initially. Must be a number larger than or equal to: " + minSpecies + "\n");
                string input = Console.ReadLine();
                species = -1;
                if (input != "") species = int.Parse(input);
                Console.Clear();
            } while (species < minSpecies);

            int minIndividuals = UserInfo.MinIndividualsAmount();
            do
            {
                Console.WriteLine("Input how individuals per species are going to be created. Must be a number larger than or equal to: " + minIndividuals + "\n");
                string input = Console.ReadLine();
                individuals = -1;
                if (input != "") individuals = int.Parse(input);
                Console.Clear();
            } while (individuals < minIndividuals);

            s.Init(years, species, individuals, dataDir, exportDir, config);
            return true;
        }
    }
}