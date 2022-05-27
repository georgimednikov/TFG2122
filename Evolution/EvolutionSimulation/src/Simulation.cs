using System;
using System.Drawing;
using System.Collections.Generic;
using EvolutionSimulation.Entities;
using System.Numerics;
using Telemetry;
using Telemetry.Events;

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
        /// <param name="worldConfig"> World configuration to generate the world map. If it is not null, no other world files are considered </param>
        virtual public void Init(int years, int species, int individuals, string dataDir, string exportDir, WorldGenConfig worldConfig)
        {
            UserInfo.SetUp(years, species, individuals, dataDir, exportDir);

#if TRACKER_ENABLED
            InitTracker();
            SetTrackerExportDir();
#endif
            // Universe Parameters
            UniverseParametersManager.ReadJSON();
            // Chromosome and ability unlocks
            Genetics.CreatureChromosome.SetChromosome();
            // Similarity Gene Weight
            Genetics.GeneticTaxonomy.SetTaxonomy();
            // World
            world = new World();
            // If there is no custom world configuration
            if (worldConfig == null)
            {
                string worldData = UserInfo.WorldFile();
                string regionData = UserInfo.RegionFile();
                // If a simulation world is provided, that one is used
                if (worldData != null && regionData != null)
                {
                    world.Init(worldData, regionData);
                    UserInfo.SetMapSize(world.map.GetLength(0));
                }
                else // A new one has to be created from scratch with the given size and default settings
                {
                    WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                    {
                        mapSize = UserInfo.Size
                    };

                    world.Init(config);
                }
            }
            else // There is a custom world configuration, which in this case means that a height map is provided
            {
                world.Init(worldConfig);
                UserInfo.SetMapSize(worldConfig.mapSize);
            }
        }

        // TODO: poder pasar el config a este init
        /// <summary>
        /// Sets up the program with the information provided by the user
        /// </summary>
        /// <param name="years"> Years to simulate </param>
        /// <param name="species"> Initial number of original species </param>
        /// <param name="individuals"> Initial number of creatures per original specie </param>
        /// <param name="worldConfig"> World configuration to generate the world map. If it is not null, no other world files are considered </param>
        /// <param name="uniParamsFile"> Raw file with the parameters of the simulation universe. If not provided, default information is setted </param>
        /// <param name="chromosomeFile"> Raw File with the chromosome information </param>
        /// <param name="sGeneWeightFile"> Raw file with each genes' weight for the chromosome </param>
        /// <param name="abilitiesFile"> Raw file with each ability unlock percentage. If not provided, default information is setted</param>
        /// <param name="exportDir"> Directory where the files will be stored when de simulation ends. If not provided, default export directory is setted</param>
        virtual public void Init(int years, int species, int individuals, string exportDir, WorldGenConfig worldConfig, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string regionMapFile = null)
        {
            UserInfo.SetUp(years, species, individuals, _exportDir: exportDir);

#if TRACKER_ENABLED
            InitTracker();
            SetTrackerExportDir();
#endif
            // Universe Parameters
            UniverseParametersManager.ReadJSON(uniParamsFile);
            // Chromosome and ability unlocks
            Genetics.CreatureChromosome.SetChromosome(chromosomeFile, abilitiesFile);
            // Similarity Gene Weight
            Genetics.GeneticTaxonomy.SetTaxonomy(sGeneWeightFile);

            // World
            world = new World();
            // If there is no custom world configuration
            if (worldConfig == null)
            {
                // If a simulation world is provided, that one is used
                if (worldFile != null && regionMapFile != null)
                {
                    world.Init(worldFile, regionMapFile);
                    UserInfo.SetMapSize(world.map.GetLength(0));
                }
                else // A new one has to be created from scratch with the given size and default settings
                {
                    WorldGenConfig config = new WorldGenConfig(World.MapType.Default)
                    {
                        mapSize = UserInfo.Size
                    };

                    world.Init(config);
                }
            }
            else // There is a custom world configuration, which in this case means that a height map is provided
            {
                world.Init(worldConfig);
                UserInfo.SetMapSize(worldConfig.mapSize);
            }
        }

        /// <summary>
        /// Performs the simulation for the established.
        /// Init must be called before running the simulation, to set the years of simulation
        /// and get the simulation information from the provided files.
        /// </summary>
        virtual public void Run()
        {
            Begin();
            Simulate();
            End();
        }

        /// <summary>
        /// The beginning of the simulation, where the original creatures
        /// are created and the number of ticks to be simulated are calculated.
        /// </summary>
        virtual protected void Begin()
        {
            CreateCreatures();
            totalTicks = world.YearToTick(UserInfo.Years);

#if TRACKER_ENABLED
            SimulationStartTrack();
#endif
        }

        /// <summary>
        /// Performs a step of the simulation
        /// </summary>
        /// <returns> False if no creatures remain, true otherwise </returns>
        virtual protected bool Step()
        {
            bool ret = world.Tick();
#if TRACKER_ENABLED
            HourTrack();
#endif
            return ret;
        }

        /// <summary>
        /// Performs all the ticks of the simulation until no creatures remain
        /// </summary>
        protected void Simulate()
        {
            while (Step() && currentTick <= totalTicks);
        }

        /// <summary>
        /// The end of the simulation. The simulation results are exported at the end
        /// </summary>
        virtual protected void End()
        {
#if TRACKER_ENABLED
            SimulationEndTrack();
#endif
            Export();

#if TRACKER_ENABLED
            EndTracker();
#endif
        }

        #region CreatureCreation
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
            int numTries = UserInfo.Size * UserInfo.Size;
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
                    } while (!valid && minDistanceCont < numTries);
                    //The creatures cant start in a water position
                    if (world.map[x, y].isWater)
                    {
                        validPosition = false;
                        continue;
                    }
                    //Try to be in a safe temperature position
                    if (!a.CheckTemperature(x, y) && temperatureCont < numTries)
                    {
                        validPosition = false;
                        temperatureCont++;
                        continue;
                    }
                }
                while (!validPosition);

                int j = 0;
                InitialSetUp(a, (Genetics.Gender)(j % 2));
                j++;

                a.Place(x, y);
                //The specified amount of individuals of each species is created.
                for (; j < UserInfo.Individuals; j++)
                {
                    Animal na = world.CreateCreature<Animal>(x, y, a.chromosome, a.speciesName);
                    InitialSetUp(na, (Genetics.Gender)(j % 2));
                }

                //The new position is added to the list of used.
                spawnPositions.Add(new Tuple<int, int>(x, y));
            }
            //SetUpInitialPopulation();
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
#if TRACKER_ENABLED
                c.BirthEventTrack();
#endif
                i++;
            }
        }

        virtual protected void InitialSetUp(Creature c, Genetics.Gender gender)
        {
            c.stats.CurrAge = (int)(UniverseParametersManager.parameters.adulthoodThreshold * c.stats.LifeSpan);
            c.chromosome.ModifyGender(gender);
            c.stats.Gender = gender;

#if TRACKER_ENABLED
            c.BirthEventTrack();
#endif
        }
        #endregion

        #region Exportation
        protected void Export()
        {
            world.ExportContent();
            WorldToBmp();
        }

        protected void WorldToBmp()
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
            Bitmap debugMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap miniature = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
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
                        c = Color.FromArgb((int)(c.R + ((h - thres2) / (1 - thres2) * (1 - (c.R / 255f))) * 255), (int)(c.G + ((h - thres2) / (1 - thres2) * (1 - (c.G / 255f))) * 255), (int)(c.B + ((h - thres2) / (1 - thres2) * (1 - (c.B / 255f)))) * 255);
                        SetPixel(j, i, c, floraMapMask, scale);
                    }
                    SetPixel(j, i, floraMapMask.GetPixel(j, i), miniature, scale);
                    #endregion

                    #region PlantMap
                    Plant plant = world.map[j / scale, i / scale].plant;
                    if (plant as Grass != null)
                    {
                        SetPixel(j, i, Color.DarkOliveGreen, treeMap, scale);
                        SetPixel(j, i, Color.DarkOliveGreen, miniature, scale);
                    }
                    else if (plant as Bush != null)
                    {
                        SetPixel(j, i, Color.ForestGreen, treeMap, scale);
                        SetPixel(j, i, Color.ForestGreen, miniature, scale);
                    }
                    else if (plant as Tree != null)
                    {
                        SetPixel(j, i, Color.LawnGreen, treeMap, scale);
                        SetPixel(j, i, Color.LawnGreen, miniature, scale);
                    }
                    else if (plant as EdibleTree != null)
                    {
                        SetPixel(j, i, Color.Red, treeMap, scale);
                        SetPixel(j, i, Color.Red, miniature, scale);
                    }

                    if (world.map[j / scale, i / scale].isWater)
                    {
                        SetPixel(j, i, Color.DarkBlue, holdRidgeMap, scale);
                        SetPixel(j, i, Color.DarkBlue, miniature, scale);
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

                    if (world.map[j / scale, i / scale].isWater) SetPixel(j, i, Color.Blue, debugMap, scale);
                    else SetPixel(j, i, Color.DarkGreen, debugMap, scale);
                }
            }
            double max;
            var posNum = ComputePathPos(out max);
            foreach (var item in posNum)
            {
                Vector2 vector = item.Key;
                int color = (int)(item.Value / max * 255);
                SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.FromArgb(255, 255, 255 - color, 255 - color), debugMap, scale);
            }

            for (int i = 0; i < world.deathsPos.Count; i++)
            {
                Vector2 vector = world.deathsPos[i].pos;
                switch (world.deathsPos[i].cause)
                {
                    case World.DeathCause.Temperature:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Red, debugMap, scale);
                        break;
                    case World.DeathCause.Others:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Yellow, debugMap, scale);
                        break;
                    case World.DeathCause.Retaliation:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Orange, debugMap, scale);
                        break;
                    case World.DeathCause.Starvation:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Pink, debugMap, scale);
                        break;
                    case World.DeathCause.Thirst:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.BlueViolet, debugMap, scale);
                        break;
                    case World.DeathCause.Exhaustion:
                        SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Fuchsia, debugMap, scale);
                        break;
                    default:
                        break;
                }
            }

            for (int t = 0; t < world.regionMap.Count; t++)
            {
                Vector2 vector = world.regionMap[t].spawnPoint;
                SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Black, voronoiMap, scale);
            }
            /////TODO GORDO: Hacer lo mismo de debug en el resto!!!
            treeMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Flora.png");
            floraMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/FloraProb.png");
            floraMapMask.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/TerrainTexture.png");
            heightMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Height.png");
            tempMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Temp.png");
            hMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Humidity.png");
            holdRidgeMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/HoldridgeBiome.png");
            voronoiMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/VoronoiRegions.png");
#if DEBUG
            debugMap.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Debug.png");
#endif
            miniature.Save($"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}/Map.png");

        }

        Dictionary<Vector2, int> ComputePathPos(out double max)
        {
            Dictionary<Vector2, int> posNum = new Dictionary<Vector2, int>();
            List<Vector2> v = world.pathPos;
            max = 1;
            foreach (var pos in v)
            {
                if (posNum.ContainsKey(pos))
                {
                    int tMax = ++posNum[pos];
                    if (tMax > max)
                        max = tMax;
                }
                else posNum.Add(pos, 1);
            }
            return posNum;
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

#if TRACKER_ENABLED
        public void InitTracker()
        {
            Tracker.Instance.Init();
            Tracker.Instance.Track(new SessionStart());
        }

        public void EndTracker()
        {
            Tracker.Instance.Track(new SessionEnd());
            Tracker.Instance.Flush();
        }

        protected void SetTrackerExportDir()
        {
            Tracker.Instance.OutputDir = UserInfo.ExportDirectory;
        }

        protected void HourTrack()
        {
            if (currentTick % UniverseParametersManager.parameters.ticksPerHour == 0)
            {
                Tracker.Instance.Track(new SimulationSample(currentTick, world.Creatures.Count, world.EatenPlants / (float)world.EdiblePlants));
                Tracker.Instance.Flush();
            }
        }


        protected void SimulationStartTrack()
        {
            Tracker.Instance.Track(new SimulationStart(world.YearToTick(1), totalTicks, world.EdiblePlants, world.map.GetLength(0)));
        }

        protected void SimulationEndTrack()
        {
            // To close creature json files
            foreach (Creature c in world.Creatures.Values)
                Tracker.Instance.Track(new CreatureDeath(world.CurrentTick, c.ID, c.speciesName, DeathType.SimulationEnd, -1, 0, c.x, c.y));
            Tracker.Instance.Track(new SimulationEnd(currentTick - 1, world.Creatures.Count, world.GetSpeciesNumber()));
        }
#endif

        protected World world;
        protected int totalTicks;
        protected int currentTick { get => world.CurrentTick; } // TODO: esta variable es redundante pero esq estaba repetida en varios sitios
    }
}
