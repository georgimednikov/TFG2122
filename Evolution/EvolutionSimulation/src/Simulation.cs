using System;
using System.Collections.Generic;
using EvolutionSimulation.Entities;

namespace EvolutionSimulation
{
    /// <summary>
    /// Simulation of evolution
    /// </summary>
    public class Simulation : ISimulation
    {
        /// <summary>
        /// Initializes the program with information provided by the user
        /// If no directories are provided, data will be looked for in the directory where the .exe is located
        /// </summary>
        /// <param name="years"> Years to simulate </param>
        /// <param name="species"> Initial number of original species </param>
        /// <param name="individuals"> Initial number of creatures per original specie </param>
        /// <param name="dataDir"> Directory where all the files with the simulation info are stored </param>
        /// <param name="exportDir"> Directory where the files will be stored when de simulation ends </param>
        virtual public void Init(int years, int species, int individuals, string dataDir, string exportDir)
        {
            UserInfo.SetUp(years, species, individuals, dataDir, exportDir);
            // Universe Parameters
            UniverseParametersManager.ReadJSON();
            // Chromosome and ability unlocks
            Genetics.CreatureChromosome.SetChromosome();
            // Similarity Gene Weight
            Genetics.GeneticTaxonomy.SetTaxonomy();
            // World
            world = new World();
            string worldData = UserInfo.WorldConfigFile();
            if (worldData != null)
            {
                WorldGenConfig worldConfig;
                // TODO: hacerlo asi o poner otro parametro para diferenciar un json de configuracion y de mundo como tal
                try 
                { 
                    worldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldGenConfig>(worldData);
                    world.Init(worldConfig);
                }
                catch( Newtonsoft.Json.JsonSerializationException e)
                {
                    world.Init(worldData);
                }

            }
            else
            {
                WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                {
                    mapSize = 512
                };

                world.Init(config);
            }

            CreateCreatures();
        }

        /// <summary>
        /// Sets up the program with the information provided by the user
        /// </summary>
        /// <param name="years"> Years to simulate </param>
        /// <param name="species"> Initial number of original species </param>
        /// <param name="individuals"> Initial number of creatures per original specie </param>
        /// <param name="uniParamsFile"> Raw file with the parameters of the simulation universe. If not provided, default information is setted </param>
        /// <param name="chromosomeFile"> Raw File with the chromosome information </param>
        /// <param name="sGeneWeightFile"> Raw file with each genes' weight for the chromosome </param>
        /// <param name="minSimilarityFile"> Raw file with the species' similarity parameter </param>
        /// <param name="abilitiesFile"> Raw file with each ability unlock percentage. If not provided, default information is setted</param>
        /// <param name="exportDir"> Directory where the files will be stored when de simulation ends. If not provided, default export directory is setted</param>
        virtual public void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string minSimilarityFile = null, string worldFile = null, string exportDir = null)
        {
            UserInfo.SetUp(years, species, individuals, _exportDir: exportDir);
            // Universe Parameters
            UniverseParametersManager.ReadJSON(uniParamsFile);
            // Chromosome and ability unlocks
            Genetics.CreatureChromosome.SetChromosome(chromosomeFile, abilitiesFile);
            // Similarity Gene Weight
            Genetics.GeneticTaxonomy.SetTaxonomy(sGeneWeightFile, minSimilarityFile);
            // World
            world = new World();
            if (worldFile != null)
            {
                WorldGenConfig worldConfig;
                try
                {
                    worldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldGenConfig>(worldFile);
                    world.Init(worldConfig);
                }
                catch (Newtonsoft.Json.JsonSerializationException e)
                {
                    world.Init(worldFile);
                }
            }
            else
            {
                WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                {
                    mapSize = 512
                };

                world.Init(config);
            }

            CreateCreatures();
        }

        virtual public void Run()
        {
            int ticks = world.YearToTick(UserInfo.Years);
            int i = 1;
            for (; i <= ticks; i++)
            {
                if (!world.Tick()) break;
            }
        }

        virtual public void Export()
        {
            world.ExportContent();
        }

        virtual protected void CreateCreatures()
        {
            //A minimum distance to leave in between species spawn points to give them some room.
            //Calculated based on the world size and amount of species to spawn, and then reduced by
            //a value to give room in the world and not fill it in a homogenous manner.
            int minSpawnDist = UserInfo.Size / UserInfo.Species / 15;

            //List with previous spawn positions, to know if a new spot is too close to another one used.
            List<Tuple<int, int>> spawnPositions = new List<Tuple<int, int>>();
            int x, y;

            for (int i = 0; i < UserInfo.Species; i++)
            {
                bool validPosition;
                do
                {
                    validPosition = true;
                    x = RandomGenerator.Next(0, UserInfo.Size);
                    y = RandomGenerator.Next(0, UserInfo.Size);

                    foreach (Tuple<int, int> p in spawnPositions)
                    {
                        Vector2Int dist = new Vector2Int(x - p.Item1, y - p.Item2);
                        if (world.map[x, y].isWater || dist.Magnitude() < minSpawnDist)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
                while (!validPosition);

                //The specified amount of individuals of each species is created.
                Animal a = world.CreateCreature<Animal>(x, y);
                for (int j = 1; j < UserInfo.Individuals; j++)
                {
                    world.CreateCreature<Animal>(x, y, a.chromosome, a.speciesName);
                }

                //The new position is added to the list of used.
                spawnPositions.Add(new Tuple<int, int>(x, y));
            }

        }

        protected World world;
    }
}
