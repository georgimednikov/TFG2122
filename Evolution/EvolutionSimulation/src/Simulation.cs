using System;
using System.Collections.Generic;
using EvolutionSimulation.Entities;
using System.IO;
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
        /// <param name="worldConfig"> World configuration to generate the world map. If it is provided, no other world files are considered </param>
        virtual public void Init(int years, int species, int individuals, string dataDir, string exportDir, WorldGenConfig worldConfig)
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

            // If the is no custom world configuration
            if (worldConfig == null)
            {
                string worldData = UserInfo.WorldFile();
                string regionData = UserInfo.RegionFile();
                // If a simulation world is provided, that one is used.
                if (worldData != null && regionData != null)
                {
                    world.Init(worldData, regionData);
                }
                else // Else a new one has to be created from scratch with the given parameters, in this case only size.
                {
                    WorldGenConfig config = new WorldGenConfig(World.MapType.Atoll)
                    {
                        mapSize = UserInfo.Size
                    };

                    world.Init(config);
                }
            }
            else // There is a custom world configuration, which in this case means that a height map is provided.
                world.Init(worldConfig);

            UniverseParametersManager.WriteDefaultParameters();
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
        /// <param name="abilitiesFile"> Raw file with each ability unlock percentage. If not provided, default information is setted</param>
        /// <param name="exportDir"> Directory where the files will be stored when de simulation ends. If not provided, default export directory is setted</param>
        virtual public void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string regionMap = null, string exportDir = null)
        {
            UserInfo.SetUp(years, species, individuals, _exportDir: exportDir);
            // Universe Parameters
            UniverseParametersManager.ReadJSON(uniParamsFile);
            // Chromosome and ability unlocks
            Genetics.CreatureChromosome.SetChromosome(chromosomeFile, abilitiesFile);
            // Similarity Gene Weight
            Genetics.GeneticTaxonomy.SetTaxonomy(sGeneWeightFile);
            // World
            world = new World();
            if (worldFile != null && regionMap != null)
            {
                world.Init(worldFile, regionMap);
            }
            else
            {
                WorldGenConfig config = new WorldGenConfig(World.MapType.Swamp)
                {
                    mapSize = UserInfo.Size
                };

                world.Init(config);
            }
            UserInfo.Size = world.map.GetLength(0);
            CreateCreatures();
        }

        virtual public void Run()
        {
            int ticks = world.YearToTick(UserInfo.Years);
            int i = 1;
            for (; i <= ticks; i++)
            {
                if (!world.Tick(i)) break;
            }
        }

        virtual public void Export()
        {
            world.ExportContent();
        }
        virtual public void ApocalypseExport(int cont)
        {
            world.ApocalypseExportContent(cont);
        }


        /*//Method to test
        virtual protected void CreateCreaturesTest()
        {
            Animal a = world.CreateCreature<Animal>(10, 10);
            a.chromosome.ModifyGender(Genetics.Gender.Male);
            Animal b = world.CreateCreature<Animal>(10, 10, a.chromosome, a.speciesName);
            b.chromosome.ModifyGender(Genetics.Gender.Female);

            Genetics.CreatureChromosome childC = Genetics.GeneticFunctions.UniformCrossover(a.chromosome, b.chromosome);
            // Mutate the chromosome
            Genetics.GeneticFunctions.UniformMutation(ref childC, UniverseParametersManager.parameters.mutationChance);
            // The new creature's pos (near to the parents)

            childC.ModifyGender(Genetics.Gender.Female);
            Animal c = b.world.CreateCreature<Animal>(10, 10, childC, b.speciesName, a.ID, b.ID);
            Animal c2 = world.CreateCreature<Animal>(10, 10, a.chromosome, a.speciesName);
            c2.chromosome.ModifyGender(Genetics.Gender.Male);

            Genetics.CreatureChromosome childD = Genetics.GeneticFunctions.UniformCrossover(a.chromosome, b.chromosome);
            // Mutate the chromosome
            Genetics.GeneticFunctions.UniformMutation(ref childD, UniverseParametersManager.parameters.mutationChance);
            // The new creature's pos (near to the parents)

            Animal d = b.world.CreateCreature<Animal>(10, 10, childD, b.speciesName, a.ID, b.ID);
            
            //world.ApocalypseExportContent(0);

            Genetics.CreatureChromosome childCC = Genetics.GeneticFunctions.UniformCrossover(c2.chromosome, c.chromosome);
            // Mutate the chromosome
            Genetics.GeneticFunctions.UniformMutation(ref childCC, UniverseParametersManager.parameters.mutationChance);
            // The new creature's pos (near to the parents)

            Animal ac2 = c.world.CreateCreature<Animal>(10, 10, childCC, c.speciesName, c2.ID, c.ID);

            Genetics.CreatureChromosome childCC2 = Genetics.GeneticFunctions.UniformCrossover(c2.chromosome, c.chromosome);
            // Mutate the chromosome
            Genetics.GeneticFunctions.UniformMutation(ref childCC2, UniverseParametersManager.parameters.mutationChance);
            // The new creature's pos (near to the parents)

            Animal ac = c.world.CreateCreature<Animal>(10, 10, childCC2, c.speciesName, c2.ID, c.ID);

            Genetics.CreatureChromosome childCC3 = Genetics.GeneticFunctions.UniformCrossover(c2.chromosome, c.chromosome);
            // Mutate the chromosome
            Genetics.GeneticFunctions.UniformMutation(ref childCC3, UniverseParametersManager.parameters.mutationChance);
            // The new creature's pos (near to the parents)

            Animal ac3 = c.world.CreateCreature<Animal>(10, 10, childCC3, c.speciesName, c2.ID, c.ID);

            world.ApocalypseExportContent(0);
        }*/

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
                Animal a = world.CreateCreature<Animal>(0, 0);
                int cont = 0;
                do
                {
                    validPosition = true;
                    x = RandomGenerator.Next(0, UserInfo.Size);
                    y = RandomGenerator.Next(0, UserInfo.Size);
                    if (world.map[x, y].isWater)
                    {
                        validPosition = false;
                        continue;
                    }
                    if (!a.CheckTemperature(x, y) && cont < UserInfo.Size)
                    {
                        validPosition = false;
                        cont++;
                        continue;
                    }
                }
                while (!validPosition);

                a.Place(x, y);
                //The specified amount of individuals of each species is created.
                for (int j = 1; j < UserInfo.Individuals; j++)
                {
                    world.CreateCreature<Animal>(x, y, a.chromosome, a.speciesName);
                }

                //The new position is added to the list of used.
                spawnPositions.Add(new Tuple<int, int>(x, y));
            }
            SetUpInitialPopulation();
        }

        /// <summary>
        /// The initial population start being adult and the half is male and the other is female
        /// </summary>
        virtual protected void SetUpInitialPopulation()
        {
            int i = 0;
            foreach (Creature c in world.Creatures.Values)
            {
                c.stats.CurrAge = (int)(UniverseParametersManager.parameters.adulthoodThreshold * c.stats.LifeSpan);
                if (i % 2 == 0)
                {
                    c.chromosome.ModifyGender(Genetics.Gender.Male);
                    c.stats.Gender = Genetics.Gender.Male;
                }
                else
                {
                    c.chromosome.ModifyGender(Genetics.Gender.Female);
                    c.stats.Gender = Genetics.Gender.Female;
                }
                i++;
            }
        }

        protected World world;
    }
}
