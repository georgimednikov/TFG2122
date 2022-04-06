using System;
using System.IO;

namespace EvolutionSimulation
{
    static public class UserInfo
    {
        static public int Size = 512;
        static public int Years = 10;
        static public int Species = 10;
        static public int Individuals = 10;

        static public string DataDirectory = "./";
        static public string ExportDirectory = "./";

        /// <summary>
        /// Returns the raw universe parameters file, if the file does not exist, returns null
        /// </summary>
        static public string UniverseParametersFile() { return LoadFile("UniverseParameters.json"); }
        /// <summary>
        /// Returns the raw chromosome file, if the file does not exist, returns null
        /// </summary>
        static public string ChromosomeFile() { return LoadFile("Chromosome.json"); }
        /// <summary>
        /// Returns the raw ability unlocks file, if the file does not exist, returns null
        /// </summary>
        static public string AbilityUnlockFile() { return LoadFile("AbilityUnlock.json"); }
        /// <summary>
        /// Returns the raw gene similarity file, if the file does not exist, returns null
        /// </summary>
        static public string GeneSimilarityFile() { return LoadFile("SimilarityGeneWeight.json"); }
        /// <summary>
        /// Returns the raw species similarity file, if the file does not exist, returns null
        /// </summary>
        static public string SpeciesSimilarityFile() { return LoadFile("SimilaritySpecies.json"); }     
        /// <summary>
        /// Returns the raw world config file, if the file does not exist, returns null
        /// </summary>
        static public string WorldConfigFile() { return LoadFile("WorldConfig.json"); }
        /// <summary>
        /// Returns the raw world file, if the file does not exist, returns null
        /// </summary>
        static public string WorldFile() { return LoadFile("World.json"); }
        /// <summary>
        /// Returns the Astar region file, if the file does not exist, returns null
        /// </summary>
        static public string RegionFile() { return LoadFile("HighMap.json"); }

        /// <summary>
        /// Loads a file form the Data directory and returns its raw data.
        /// If the file is not in the directory, it returns null.
        /// </summary>
        static private string LoadFile(string fileName)
        {
            string filePath = DataDirectory + fileName;
            if (!File.Exists(filePath)) return null;
            return File.ReadAllText(filePath);
        }      

        /// <summary>
        /// Sets up the program with the file information provided by the user
        /// If no directories are provided, data will be looked for in the directory where the .exe is located
        /// </summary>
        /// <param name="years"> Years to simulate </param>
        /// <param name="species"> Initial number of original species </param>
        /// <param name="individuals"> Initial number of creatures per original specie </param>
        /// <param name="_dataDir"> Directory where all the files with the simulation info are stored </param>
        /// <param name="_exportDir"> Directory where the files will be stored when de simulation ends </param>
        static public void SetUp(int years, int species, int individuals, string _dataDir = null, string _exportDir = null)
        {
            Years = years;
            Species = species;
            Individuals = individuals;
            if(_dataDir != null) DataDirectory = _dataDir;
            if(_exportDir != null)ExportDirectory = _exportDir;
        }

        // TODO: Estos metodos en la simulacion
        static public int MinWorldSize() { return 100; }
        static public int MinSpeciesAmount() { return Size / 20; }
        static public int MinIndividualsAmount() { return 2; }
    }
}
