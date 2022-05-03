using System;
using System.Drawing;
using System.Collections.Generic;
using EvolutionSimulation.Entities;
using System.Numerics;
using System.IO;
using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation
{
    /// <summary>
    /// Simulation of evolution
    /// </summary>
    public class Simulation : ISimulation
    {
        System.Timers.Timer timer;

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
            InitTracker();

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
                    WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                    {
                        mapSize = UserInfo.Size
                    };

                    world.Init(config);
                }
            }
            else // There is a custom world configuration, which in this case means that a height map is provided.
                world.Init(worldConfig);

            //WorldToBmp();
            CreateCreatures();

            //StartFlusingTracker(5000);
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
            InitTracker();

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
                WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                {
                    mapSize = UserInfo.Size
                };

                world.Init(config);
            }
            UserInfo.Size = world.map.GetLength(0);
            //WorldToBmp();
            CreateCreatures();
            //StartFlusingTracker(5000);
        }
        private void InitTracker()
        {
            Tracker.Instance.Init();
            Tracker.Instance.Track(new SessionStart());
        }

        /// <summary>
        /// Seconds between each flush
        /// </summary>
        /// <param name="miliseconds"></param>
        private void StartFlusingTracker(int miliseconds)
        {
            timer = new System.Timers.Timer(5000);
            // TODO: Se puede hacer que sea el propio tracker el que haga el flush automatico cada x tiempo
            timer.Elapsed += (o, args) => { Tracker.Instance.Flush(); };
            timer.AutoReset = true;
            timer.Start();
        }

        private void EndTracker()
        {
            timer.Stop();
            timer.Dispose();
            Tracker.Instance.Track(new SessionEnd());
            Tracker.Instance.Flush();
        }

        virtual public void Run()
        {
            int YearTicks = world.YearToTick(1.0f);
            DateTime prevT = DateTime.Now;
            DateTime newT = DateTime.Now;
            int ticks = world.YearToTick(UserInfo.Years);
            int i = 1;
            double prevN = 0;
            int apocalypsisCont = 0, lastNum = world.Creatures.Count, births = 0, birthCur = 0;

            String s = "[";
            for (int j = 0; j < prevN; j++)
            {
                s += ".";
            }
            for (int j = (int)prevN; j < 100; j++)
            {
                s += " ";
            }
            Console.Clear();
            Console.WriteLine(s + "] " + prevN + "%");

            for (; i <= ticks; i++)
            {
                if (!world.Tick(i))
                {
                    break; //TODO: NO DEJAR ESTO
                    ApocalypseExport(apocalypsisCont++);
                    Console.WriteLine("APOCALYPSIS: Generating new set of creatures");
                    CreateCreatures();
                    birthCur = 0;
                    lastNum = world.Creatures.Count;
                };

                //if (lastNum < world.Creatures.Count) { births += world.Creatures.Count - lastNum; birthCur += world.Creatures.Count - lastNum; }
                //lastNum = world.Creatures.Count;
                //Console.WriteLine("Num Creatures: {1} Apocalypsis: {3} Births: {4} Births(Current Apocalypsis): {5} Ticks: {0}/{2}", i, world.Creatures.Count, ticks, apocalypsisCont, births, birthCur);
                //Console.WriteLine("Num Creatures: {1} Ticks: {0}/{2} ", i, world.Creatures.Count, ticks);
                //Render();
                //Thread.Sleep(1000);
                if (i % UniverseParametersManager.parameters.ticksPerHour == 0)
                {
                    Tracker.Instance.Track(new SimulationSample(i, world.Creatures.Count));
                    Tracker.Instance.Flush();
                }
                if (i % YearTicks == 0)
                    Console.WriteLine("A Year has passed");

                double newN = Math.Round((double)i / (double)ticks * 100);
                if (newN > prevN)
                {
                    newT = DateTime.Now;
                    prevN = newN;
                    String tempS = "[";
                    for (int j = 0; j < prevN; j++)
                    {
                        tempS += ".";
                    }
                    for (int j = (int)prevN; j < 100; j++)
                    {
                        tempS += " ";
                    }

                    Console.Clear();
                    Console.WriteLine(tempS + "] " + prevN + "%");
                    if (lastNum < world.Creatures.Count) { births += world.Creatures.Count - lastNum; }
                    lastNum = world.Creatures.Count;
                    Console.WriteLine("Ticks: {0}/{2} | Num Creatures: {1} | Births: {3} | Entities to Update: {4} | {5}", i, world.Creatures.Count, ticks, births, world.StaticEntitiesToUpdate.Count, newT - prevT);
                    prevT = newT;
                }
            }
            // Para dejar los json bien cuando termine la simulacion
            foreach (Creature c in world.Creatures.Values)
                Tracker.Instance.Track(new CreatureDeath(ticks, c.ID, c.speciesName, DeathType.SimulationEnd));

            Console.WriteLine("Deaths by: Temperature {0} Damage by others {1} Retaliation {2} Starvation {3} Thirst {4} Exhaustion {5} Poison {6}", world.deaths[0], world.deaths[1], world.deaths[2], world.deaths[3], world.deaths[4], world.deaths[5], world.deaths[6]);
            Console.Write("Simulation ended, ticks elapsed: " + i + "\n");
            Tracker.Instance.Track(new SessionEnd());
            Tracker.Instance.Flush();
        }

        virtual public void Export()
        {
            //EndTracker();
            world.ExportContent();
            Console.Write("Simulation data has been exported");
        }
        virtual public void ApocalypseExport(int cont)
        {
            world.ApocalypseExportContent(cont);
            Console.Write("Simulation data has been exported because of Apocalysis");
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
            int minSpawnDist = UserInfo.Size / UserInfo.Species;

            //List with previous spawn positions, to know if a new spot is too close to another one used.
            List<Tuple<int, int>> spawnPositions = new List<Tuple<int, int>>();
            int x, y;
            bool validPosition;
            bool valid;
            Animal a;
            int temperatureCont;
            int minDistanceCont;
            for (int i = 0; i < UserInfo.Species; i++)
            {
                a = world.CreateCreature<Animal>(0, 0);
                temperatureCont = 0;//a cont to create the creatures in a position that is not receiving damage by temperature
                minDistanceCont = 0;//a cont to create the creatures separated if possible
                //Find a good position to start for the creature. That means with a minimun distance with other creatures,
                //not in a water tile and in a position that is with a confortable temperature to the creature
                do
                {
                    validPosition = true;
                    valid = true;
                    //Try to separate the creatures to avoid them starting attacking and dying in the beginning
                    do
                    {
                        x = RandomGenerator.Next(0, UserInfo.Size);
                        y = RandomGenerator.Next(0, UserInfo.Size);
                        foreach (Tuple<int, int> pos in spawnPositions)
                        {
                            if (Math.Abs(pos.Item1 - x) < minSpawnDist && Math.Abs(pos.Item2 - y) < minSpawnDist)
                            {
                                valid = false;
                                break;
                            }
                        }
                        minDistanceCont++;
                    } while (!valid && minDistanceCont < UserInfo.Size);
                    //The creatures cant start in a water position
                    if (world.map[x, y].isWater)
                    {
                        validPosition = false;
                        continue;
                    }
                    //Try to be in a safe temperature position
                    if (!a.CheckTemperature(x, y) && temperatureCont < UserInfo.Size)
                    {
                        validPosition = false;
                        temperatureCont++;
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

        #region BMP
        public void WorldToBmp()
        {
            int scale = 4;
            Bitmap treeMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMapMask = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap heightMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap tempMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap hMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap holdRidgeMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap voronoiMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            double val;
            for (int i = 0; i < world.map.GetLength(0) * scale; i += scale)
            {
                for (int j = 0; j < world.map.GetLength(1) * scale; j += scale)
                {
                    #region HeightMap
                    val = world.map[j / scale, i / scale].height;
                    if (val < 0.3) SetPixel(j, i, Color.DarkBlue, heightMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Blue, heightMap, scale);
                    else if (val == 0.5) SetPixel(j, i, Color.DarkGreen, heightMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Green, heightMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.Yellow, heightMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.LightYellow, heightMap, scale);
                    else SetPixel(j, i, Color.White, heightMap, scale);
                    val = world.map[j / scale, i / scale].humidity;
                    #endregion

                    #region HumidityMap
                    if (val < 0.3) SetPixel(j, i, Color.DarkRed, hMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Red, hMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.IndianRed, hMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.MediumVioletRed, hMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.Blue, hMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.DarkBlue, hMap, scale);
                    #endregion

                    #region TemperatureMap
                    val = world.map[j / scale, i / scale].temperature;
                    if (val < 0.2) SetPixel(j, i, Color.DarkBlue, tempMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.Blue, tempMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, tempMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Orange, tempMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.OrangeRed, tempMap, scale);
                    else SetPixel(j, i, Color.Red, tempMap, scale);
                    #endregion

                    #region FloraMap
                    val = world.map[j / scale, i / scale].flora;
                    if (val == 0)
                        if (world.map[j / scale, i / scale].isWater)
                        {
                            SetPixel(j, i, Color.DarkBlue, treeMap, scale);
                            SetPixel(j, i, Color.DarkBlue, floraMap, scale);
                        }
                        else SetPixel(j, i, Color.Black, floraMap, scale);
                    else if (val < 0.1) SetPixel(j, i, Color.DarkRed, floraMap, scale);
                    else if (val < 0.2) SetPixel(j, i, Color.Red, floraMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.OrangeRed, floraMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Orange, floraMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, floraMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.YellowGreen, floraMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.Green, floraMap, scale);
                    else SetPixel(j, i, Color.White, floraMap, scale);
                    #endregion

                    #region TerrainTexture

                    float thres = 1.0f, thres2 = 0.7f;
                    double h = world.map[j / scale, i / scale].height;

                    if (val >= thres) SetPixel(j, i, Color.FromArgb(0, 255, 0), floraMapMask, scale);
                    else SetPixel(j, i, Color.FromArgb((int)(150 * (thres - val)), (int)(90 + (val * 165f / thres)), 0), floraMapMask, scale);


                    if (h >= thres2)
                    {
                        Color c = floraMapMask.GetPixel(j, i);
                        SetPixel(j, i, Color.FromArgb((int)(c.R + ((h - thres2) / (1 - thres2) * (1 - (c.R / 255f))) * 255), (int)(c.G + ((h - thres2) / (1 - thres2) * (1 - (c.G / 255f))) * 255), (int)(c.B + ((h - thres2) / (1 - thres2) * (1 - (c.B / 255f)))) * 255), floraMapMask, scale);
                    }
                    #endregion

                    #region PlantMap
                    Plant plant = world.map[j / scale, i / scale].plant;
                    if (plant as Grass != null)
                        SetPixel(j, i, Color.DarkOliveGreen, treeMap, scale);
                    else if (plant as Bush != null)
                        SetPixel(j, i, Color.ForestGreen, treeMap, scale);
                    else if (plant as Tree != null)
                        SetPixel(j, i, Color.LawnGreen, treeMap, scale);
                    else if (plant as EdibleTree != null)
                        SetPixel(j, i, Color.Red, treeMap, scale);

                    if (world.map[j / scale, i / scale].isWater)
                    {
                        SetPixel(j, i, Color.FromArgb(0, 0, 255), holdRidgeMap, scale);

                    }
                    #endregion

                    #region HoldridgeMap
                    val = world.map[j / scale, i / scale].temperature;
                    double val2 = world.map[j / scale, i / scale].humidity;
                    //Mapa usando Holdridge de 39 Biomas
                    //Polar
                    if (val < 0.1)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(191, 191, 191), holdRidgeMap, scale); //Polar Desert
                        else SetPixel(j, i, Color.FromArgb(255, 255, 255), holdRidgeMap, scale); //Polar Ice
                    }
                    //Subpolar
                    else if (val < 0.2)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(128, 128, 128), holdRidgeMap, scale); //Subpolar Dry Tundra
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(96, 128, 128), holdRidgeMap, scale); //Subpolar Moist Tundra
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(64, 128, 128), holdRidgeMap, scale); //Subpolar Wet Tundra
                        else SetPixel(j, i, Color.FromArgb(32, 128, 128), holdRidgeMap, scale); //Subpolar Rain Tundra
                    }
                    //Boreal
                    else if (val < 0.3)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(160, 160, 128), holdRidgeMap, scale); //Boreal Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(128, 160, 128), holdRidgeMap, scale); //Boreal Dry Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(96, 160, 128), holdRidgeMap, scale); //Boreal Moist Forest
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(64, 160, 144), holdRidgeMap, scale); //Boreal Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 160, 192), holdRidgeMap, scale); //Boreal Rain Forest
                    }
                    //Cool Temperate
                    else if (val < 0.35)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(192, 192, 128), holdRidgeMap, scale); //Cool Temperate Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(160, 192, 128), holdRidgeMap, scale); //Cool Temperate Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(128, 192, 128), holdRidgeMap, scale); //Cool Temperate Steppe
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(96, 192, 128), holdRidgeMap, scale); //Cool Temperate Moist Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(64, 192, 144), holdRidgeMap, scale); //Cool Temperate Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 192, 192), holdRidgeMap, scale); //Cool Temperate Rain Forest
                    }
                    //Warm Temperate
                    else if (val < 0.65)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(224, 224, 128), holdRidgeMap, scale); //Warm Temperate Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(192, 224, 128), holdRidgeMap, scale); //Warm Temperate Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(160, 224, 128), holdRidgeMap, scale); //Warm Temperate Thorn Scrub
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(128, 224, 128), holdRidgeMap, scale); //Warm Temperate Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(96, 224, 128), holdRidgeMap, scale); //Warm Temperate Moist Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(64, 224, 144), holdRidgeMap, scale); //Warm Temperate Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 224, 192), holdRidgeMap, scale); //Warm Temperate Rain Forest
                    }
                    //Subtropical
                    else if (val < 0.9)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(240, 240, 128), holdRidgeMap, scale); //Subtropical Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(208, 240, 128), holdRidgeMap, scale); //Subtropical Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(176, 240, 128), holdRidgeMap, scale); //Subtropical Thorn Woodland
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(128, 240, 128), holdRidgeMap, scale); //Subtropical Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(96, 240, 128), holdRidgeMap, scale); //Subtropical Moist Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(64, 240, 144), holdRidgeMap, scale); //Subtropical Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 240, 176), holdRidgeMap, scale); //Subtropical Rain Forest
                    }
                    //Tropical
                    else
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(255, 255, 128), holdRidgeMap, scale); //Tropical Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(224, 255, 128), holdRidgeMap, scale); //Tropical Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(192, 255, 128), holdRidgeMap, scale); //Tropical Thorn Woodland
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(160, 255, 128), holdRidgeMap, scale); //Tropical Very Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(128, 255, 128), holdRidgeMap, scale); //Tropical Dry Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(96, 255, 128), holdRidgeMap, scale); //Tropical Moist Forest
                        else if (val2 < 0.95) SetPixel(j, i, Color.FromArgb(60, 255, 144), holdRidgeMap, scale); //Tropical Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 255, 160), holdRidgeMap, scale); //Tropical Rain Forest
                    }
                    #endregion

                    #region VoronoiDiagram
                    val = world.map[j / scale, i / scale].regionId;
                    switch (val % 20)
                    {
                        case 0:
                            SetPixel(j, i, Color.Red, voronoiMap, scale);
                            break;
                        case 1:
                            SetPixel(j, i, Color.Blue, voronoiMap, scale);
                            break;
                        case 2:
                            SetPixel(j, i, Color.Green, voronoiMap, scale);
                            break;
                        case 3:
                            SetPixel(j, i, Color.Violet, voronoiMap, scale);
                            break;
                        case 4:
                            SetPixel(j, i, Color.Cyan, voronoiMap, scale);
                            break;
                        case 5:
                            SetPixel(j, i, Color.Lime, voronoiMap, scale);
                            break;
                        case 6:
                            SetPixel(j, i, Color.Brown, voronoiMap, scale);
                            break;
                        case 7:
                            SetPixel(j, i, Color.Yellow, voronoiMap, scale);
                            break;
                        case 8:
                            SetPixel(j, i, Color.Orange, voronoiMap, scale);
                            break;
                        case 9:
                            SetPixel(j, i, Color.LightGoldenrodYellow, voronoiMap, scale);
                            break;
                        case 10:
                            SetPixel(j, i, Color.Chocolate, voronoiMap, scale);
                            break;
                        case 11:
                            SetPixel(j, i, Color.Chartreuse, voronoiMap, scale);
                            break;
                        case 12:
                            SetPixel(j, i, Color.BurlyWood, voronoiMap, scale);
                            break;
                        case 13:
                            SetPixel(j, i, Color.DarkKhaki, voronoiMap, scale);
                            break;
                        case 14:
                            SetPixel(j, i, Color.DarkOrchid, voronoiMap, scale);
                            break;
                        case 15:
                            SetPixel(j, i, Color.Firebrick, voronoiMap, scale);
                            break;
                        case 16:
                            SetPixel(j, i, Color.DeepSkyBlue, voronoiMap, scale);
                            break;
                        case 17:
                            SetPixel(j, i, Color.IndianRed, voronoiMap, scale);
                            break;
                        case 18:
                            SetPixel(j, i, Color.Ivory, voronoiMap, scale);
                            break;
                        case 19:
                            SetPixel(j, i, Color.LimeGreen, voronoiMap, scale);
                            break;
                    }
                    #endregion
                    if ((j / scale) % 32 == 0 || (i / scale) % 32 == 0)
                    {
                        SetPixel(j, i, Color.White, voronoiMap, scale / 2);
                    }
                }
            }

            for (int t = 0; t < world.regionMap.Count; t++)
            {
                Vector2 vector = world.regionMap[t].spawnPoint;
                SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Black, voronoiMap, scale);
            }

            treeMap.Save("treeTest.png");
            floraMap.Save("flora.bmp");
            floraMapMask.Save("floraMask.bmp");
            heightMap.Save("height.bmp");
            tempMap.Save("temp.bmp");
            hMap.Save("humidity.bmp");
            holdRidgeMap.Save("biome.bmp");
            voronoiMap.Save("VoronoiDiagram.bmp");
        }

        void SetPixel(int x, int y, Color color, Bitmap bitmap, int scale = 2)
        {
            for (int i = 0; i < scale; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    bitmap.SetPixel(x + i, y + j, color);
                }
            }
        }
        #endregion

        protected World world;
    }
}
