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
            s.OnSimulationBegin += (e) => { loadingBar = new ConsoleLoadingBar(); loadingBar.Begin(e); };
            s.OnSimulationStep += (e) => { loadingBar.Update(e); };
#if true //TODO DEBUG
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

            // Input data directory request
            do
            {
                Console.WriteLine("Input a valid directory containing the necessary data files for the program (chromosome.json, etc.):\n");
                dataDir = Console.ReadLine() + "\\";
                Console.Clear();
            } while (!Directory.Exists(dataDir));

            // Output data directory request
            Console.WriteLine("Input a valid directory in which the resulting data will be saved:\n");
            exportDir = Console.ReadLine() + "\\";
            Console.Clear();
            if (!Directory.Exists(exportDir))
                exportDir = Directory.CreateDirectory(exportDir).FullName;

            // World information check and request if needed
            // If a simulation world is not given, a new one has to be generated.
            int minSize = UserInfo.MinWorldSize();
            WorldGenConfig config = null;
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
                }
                else // If nothing is given, a size and a map type have to be requested from the user 
                {
                    // Map size request
                    int mapSize;
                    do
                    {
                        Console.WriteLine("Input how big in squares the world is going to be. Must be a number larger than or equal to: " + minSize + "\n");
                        string input = Console.ReadLine();
                        mapSize = -1;
                        if (input != "")
                        {
                            try
                            {
                                mapSize = int.Parse(input); 
                                
                            }
                            catch {Console.WriteLine("You must introduce a number"); }
                        }
                    } while (mapSize < minSize);
                    Console.Clear();

                    // Map type request
                    World.MapType[] mapTypes = (World.MapType[])Enum.GetValues(typeof(World.MapType));
                    int index = -1;
                    int lengthWithoutLast = mapTypes.Length - 1;    // last type is custom, and cannot be provided this way
                    do
                    {
                        Console.WriteLine("Input the number of the type of map that should be generated:");
                        string separator;
                        for (int i = 0; i < lengthWithoutLast; i++)
                        {
                            separator = i % mapTypes.Length < lengthWithoutLast - 1 ? " | " : "";
                            Console.Write($"<{i}> {mapTypes[i]}{separator}");
                        }
                        Console.WriteLine();

                        string input = Console.ReadLine();
                        try
                        {
                            index = int.Parse(input);
                            if (index < 0 || index >= mapTypes.Length - 1)
                                Console.WriteLine("Incorrect type number");
                        }
                        catch { Console.WriteLine("You must introduce a number"); }
                    } while (index < 0 || index >= mapTypes.Length - 1);
                    Console.Clear();

                    config = new WorldGenConfig(mapTypes[index])
                    {
                        mapSize = mapSize
                    };
                }
            }

            // Simulation duration request
            do
            {
                Console.WriteLine("Input how many years of evolution are going to be simulated:");
                string input = Console.ReadLine();
                years = -1;
                if (input != "")
                try
                {
                    years = int.Parse(input);                   
                }
                catch { Console.WriteLine("You must introduce a number"); }
            } while (years < 0);
            Console.Clear();
           
            // Original species request
            int minSpecies = UserInfo.MinSpeciesAmount();
            do
            {
                Console.WriteLine("Input how many species are going to be created initially. Must be a number larger than or equal to: " + minSpecies + "\n");
                string input = Console.ReadLine();
                species = -1;
                try
                {
                    species = int.Parse(input);
                }
                catch { Console.WriteLine("You must introduce a number"); }
            } while (species < minSpecies);
             Console.Clear();
            
            // Initial individuals per species request
            int minIndividuals = UserInfo.MinIndividualsAmount();
            do
            {
                Console.WriteLine("Input how many individuals per species are going to be created. Must be a number larger than or equal to: " + minIndividuals + "\n");
                string input = Console.ReadLine();
                individuals = -1;
                if (input != "")
                try
                {
                    individuals = int.Parse(input);
                }
                catch { Console.WriteLine("You must introduce a number"); }
            } while (individuals < minIndividuals);
            Console.Clear();

            // Simulation initialization after all information has been provided
            s.Init(years, species, individuals, dataDir, exportDir, config);
            return true;
        }
    }
}