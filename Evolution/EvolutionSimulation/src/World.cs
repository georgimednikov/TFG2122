using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using EvolutionSimulation.Entities;
using EvolutionSimulation.Genetics;
using EvolutionSimulation.IO;
using EvolutionSimulation.Utils;


namespace EvolutionSimulation
{
    /// <summary>
    /// Object used to configurate the world generation.
    /// </summary>
    public class WorldGenConfig
    {
        /// <summary>
        /// Size of the map. If a heightMap/humidityMap/temperatureMap is provided then this parameter will be ignored.
        /// </summary>
        public int mapSize = UserInfo.Size;
        /// <summary>
        /// Array of waves used to generate the heightmap
        /// </summary>
        public World.Wave[] heightWaves;
        /// <summary>
        /// Array of waves used to generate the humidity map
        /// </summary>
        public World.Wave[] humidityWaves;
        /// <summary>
        /// Array of waves used to generate the temperature map
        /// </summary>
        public World.Wave[] temperatureWaves;
        /// <summary>
        /// Already generated heightmap. It must be with values on an interval between [0, 1].
        /// </summary>
        public float[,] heightMap;
        /// <summary>
        /// Already generated humidity map. It must be with values on an interval between [0, 1]. Beware that it will be modified by the heightmap unless specified.
        /// </summary>
        public float[,] humidityMap;
        /// <summary>
        /// Already generated temperature map. It must be with values on an interval between [0, 1]. Beware that it will be modified by the heightmap unless specified.
        /// </summary>
        public float[,] temperatureMap;
        /// <summary>
        /// Already generated region Astar map.
        /// </summary>
        public List<World.MapRegion> regionMap;
        /// <summary>
        /// Should the provided heightmap or the generated be modified by the EvaluateHeight function.
        /// </summary>
        public bool heightModifiedByFunction = true;
        /// <summary>
        /// Function used to modify the heightmap. It won't  be modified if heightNotModifiedByFunction is true. It must be defined for all input values between [0,1] and must output values between [0,1]
        /// </summary>
        public Func<double, double> evaluateHeight;
        /// <summary>
        /// Function defining how the heightmap modifies the other two maps. It must be defined for all input values between [0, 1] and must output values between [-1, 1]
        /// </summary>
        public Func<double, double> evaluateInfluence;
        /// <summary>
        /// Function defining how flora density is created using the temperatura and humidity maps. Inputs values will be humidity and temperature on each point, each between [0, 1]. It must output values between [0,1]
        /// </summary>
        public Func<double, double, double> evaluateFlora;
        /// <summary>
        /// Function defining how, by default, water softens temperature. Input values will be height on a point, temperature on the same point and average temperature pre-modification. Must output values between [0,1]
        /// </summary>
        public Func<double, double, double, double> temperatureSoftener;
        /// <summary>
        /// Function defining which flora is generated using the flora density. Input values will be between (0,1] and must output integer values between [0,3].
        /// Being 0 = Grass, 1 = Bush, 2 = Tree and 3 = Edible Tree
        /// </summary>
        public Func<double, int> floraSelector;
        /// <summary>
        /// Which type of terrain should be generated, if Custom type is not selected, only mapSize, will be used.
        /// </summary>
        public World.MapType type;
        /// <summary>
        /// Number of wholly isolated landmasses per chunk (32x32). Increase if you the generated region map does not manage to include all islands.
        /// </summary>
        public int numPasses = 2;

        public WorldGenConfig(World.MapType type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// Definition of the simulated world
    /// </summary>
    public class World
    {
        public enum DeathCause
        {
            Temperature,
            Others,
            Retaliation,
            Starvation,
            Thirst,
            Exhaustion
        }

        public int EdiblePlants { get; private set; }

#if TRACKER_ENABLED
        public int EatenPlants; // Number of plants fully eaten currently
#endif

        public struct Death
        {
            public Vector2 pos;
            public DeathCause cause;
        }

        /// <summary>
        /// Properties of each map tile
        /// </summary>
        public class MapData
        {
            public double height, humidity, temperature, flora;
            public Plant plant;
            public bool isWater;
            public int regionId = -1;
        }

        public enum MapType
        {
            Default,
            Island,
            Atoll,
            Swamp,
            Custom
        }

        public class MapRegion
        {
            public Vector2 spawnPoint;
            public Dictionary<int, List<Vector2>> links;
        }

        public void Init(string rawWorldData, string regionMap)
        {
            WorldGenConfig config = new WorldGenConfig(MapType.Custom);
            map = JsonLoader.Deserialize<MapData[,]>(rawWorldData);
            int n = map.GetLength(0);
            float[,] heightMap = new float[n, n];
            float[,] temperatureMap = new float[n, n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    heightMap[i, j] = (float)map[i, j].height;
                    temperatureMap[i, j] = (float)map[i, j].temperature;
                }
            }
            config.heightMap = heightMap;
            config.temperatureMap = temperatureMap;
            this.regionMap = JsonLoader.Deserialize<List<MapRegion>>(regionMap);
            config.regionMap = this.regionMap;
            Validator.Validate(config);
            

            ticksHour = UniverseParametersManager.parameters.ticksPerHour;
            hoursDay = UniverseParametersManager.parameters.hoursPerDay;
            daysYear = UniverseParametersManager.parameters.daysPerYear;
            dawnHour = UniverseParametersManager.parameters.morningStart * ticksHour;
            nightFallHour = UniverseParametersManager.parameters.nightStart * ticksHour;
            CurrentTick = 1;

            mapSize = map.GetLength(0);
            entityMap = new List<IEntity>[mapSize, mapSize];
            for (int yIndex = 0; yIndex < mapSize; yIndex++)
            {
                for (int xIndex = 0; xIndex < mapSize; xIndex++)
                {
                    entityMap[xIndex, yIndex] = new List<IEntity>();
                }
            }
            taxonomy = new GeneticTaxonomy();
            Creatures = new Dictionary<int, Creature>();
            metabolismComparer = new Comparers.SortByMetabolism();
            StaticEntities = new Dictionary<int, StaticEntity>();
            entitiesToDelete = new List<int>();
            StaticEntitiesToUpdate = new List<StaticEntity>();
            MapData mapData;
            // Create plant entities from the file
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    mapData = map[i, j];
                    if (mapData.plant == null) continue;

                    Plant plant = null;
                    switch (mapData.plant.type)
                    {
                        case Plant.PlantType.Tree:
                            plant = CreateStaticEntity<Tree>(i, j, 0);
                            break;
                        case Plant.PlantType.EdibleTree:
                            plant = CreateStaticEntity<EdibleTree>(i, j, UniverseParametersManager.parameters.eTreeHp);
                            EdiblePlants++;
                            break;
                        case Plant.PlantType.Bush:
                            plant = CreateStaticEntity<Bush>(i, j, UniverseParametersManager.parameters.bushHp);
                            EdiblePlants++;
                            break;
                        case Plant.PlantType.Grass:
                            plant = CreateStaticEntity<Grass>(i, j, UniverseParametersManager.parameters.grassHp);
                            EdiblePlants++;
                            break;
                    }
                    mapData.plant = plant;
                }
            }
        }

        /// <summary>
        /// Initializes the map with a matrix of provided size.
        /// </summary>
        public void Init(WorldGenConfig config)
        {
            ticksHour = UniverseParametersManager.parameters.ticksPerHour;
            hoursDay = UniverseParametersManager.parameters.hoursPerDay;
            daysYear = UniverseParametersManager.parameters.daysPerYear;
            dawnHour = UniverseParametersManager.parameters.morningStart * ticksHour;
            nightFallHour = UniverseParametersManager.parameters.nightStart * ticksHour;
            CurrentTick = 1;

            if (config == null) throw new NullReferenceException("World generation config is null");

            Validator.Validate(config);

            switch (config.type)
            {
                case MapType.Default:
                    complexGen = false;
                    numPasses = 2;
                    evaluateHeight = DefaultEvaluateHeightCurve;
                    break;
                case MapType.Custom:
                    complexGen = false;
                    if (config.evaluateHeight == null) evaluateHeight = DefaultEvaluateHeightCurve;
                    else evaluateHeight = config.evaluateHeight;
                    numPasses = config.numPasses;
                    break;
                case MapType.Island:
                    complexGen = true;
                    numPasses = 3;
                    complexEvaluateHeight = IslandEvaluateHeightCurve;
                    break;
                case MapType.Atoll:
                    complexGen = true;
                    numPasses = 3;
                    complexEvaluateHeight = AtollEvaluateHeightCurve;
                    break;
                case MapType.Swamp:
                    complexGen = true;
                    numPasses = 3;
                    modifiedHeight = false;
                    break;
            }

            //If Custom type is not selected, the config will be ignored
            int tSize = config.mapSize;
            if (config.type != MapType.Custom) config = new WorldGenConfig(config.type);
            config.mapSize = tSize;

            evaluateInfluence = (config.evaluateInfluence != null) ? config.evaluateInfluence : EvaluateInfluenceCurve;
            evaluateFlora = (config.evaluateFlora != null) ? config.evaluateFlora : EvaluateFloraCurve;
            temperatureSoftener = (config.temperatureSoftener != null) ? config.temperatureSoftener : SoftenTemperatureByHeight;
            floraSelector = (config.floraSelector != null) ? config.floraSelector : ChoosePlant;
            if (config.type == MapType.Custom) modifiedHeight = config.heightModifiedByFunction;

            taxonomy = new GeneticTaxonomy();
            Creatures = new Dictionary<int, Creature>();
            metabolismComparer = new Comparers.SortByMetabolism();
            StaticEntities = new Dictionary<int, StaticEntity>();
            entitiesToDelete = new List<int>();
            StaticEntitiesToUpdate = new List<StaticEntity>();

            p = new Perlin();
            if (config.heightMap != null) { heightMap = config.heightMap; config.mapSize = mapSize = heightMap.GetLength(0); }
            else mapSize = config.mapSize;

            if (config.heightWaves != null) heightWaves = config.heightWaves;
            else
            {
                if (complexGen)
                {
                    heightWaves = new Wave[3];
                    heightWaves[0] = new Wave();
                    heightWaves[0].seed = RandomGenerator.Next(0, 10000);
                    heightWaves[0].frequency = 1f;
                    heightWaves[0].amplitude = 1f;
                    heightWaves[1] = new Wave();
                    heightWaves[1].seed = RandomGenerator.Next(0, 10000);
                    heightWaves[1].frequency = 2f;
                    heightWaves[1].amplitude = 0.7f;
                    heightWaves[2] = new Wave();
                    heightWaves[2].seed = RandomGenerator.Next(0, 10000);
                    heightWaves[2].frequency = 4f;
                    heightWaves[2].amplitude = 0.25f;
                }
                else
                {
                    heightWaves = new Wave[1];
                    heightWaves[0] = new Wave();
                    heightWaves[0].seed = RandomGenerator.Next(0, 10000);
                    heightWaves[0].frequency = 0.5f;
                    heightWaves[0].amplitude = 1f;
                }
            }

            if (config.humidityMap != null) { humidityMap = config.humidityMap; config.mapSize = mapSize = humidityMap.GetLength(0); }
            else mapSize = config.mapSize;

            if (config.humidityWaves != null) humidityWaves = config.humidityWaves;
            else
            {
                humidityWaves = new Wave[1];
                humidityWaves[0] = new Wave();
                humidityWaves[0].seed = RandomGenerator.Next(0, 10000);
                humidityWaves[0].frequency = 0.5f;
                humidityWaves[0].amplitude = 1f;
            }


            if (config.temperatureMap != null) { temperatureMap = config.temperatureMap; config.mapSize = mapSize = temperatureMap.GetLength(0); }
            else mapSize = config.mapSize;

            if (config.temperatureWaves != null) temperatureWaves = config.temperatureWaves;
            else
            {
                temperatureWaves = new Wave[1];
                temperatureWaves[0] = new Wave();
                temperatureWaves[0].seed = RandomGenerator.Next(0, 10000);
                temperatureWaves[0].frequency = 0.25f;
                temperatureWaves[0].amplitude = 1f;
            }

            entityMap = new List<IEntity>[mapSize, mapSize];
            InitMap();
            if (config.regionMap == null)
            {
                chunkSize = 32;
                regionMap = new List<MapRegion>();
                FillRegionMap();
            }
            else
            {
                regionMap = config.regionMap;
            }
        }

        private void FillRegionMap()
        {
            int numReg = 0;
            Queue<Vector2> regions = new Queue<Vector2>(); 
            for (int pass = 0; pass < numPasses; pass++)
            {
                for (int i = 0; i < map.GetLength(0) / chunkSize; i++)
                {
                    for (int j = 0; j < map.GetLength(1) / chunkSize; j++)
                    {
                        int x, y;
                        int tries = chunkSize * chunkSize;
                        do
                        {
                            tries--;
                            x = (int)((i + RandomGenerator.NextDouble()) * chunkSize);
                            y = (int)((j + RandomGenerator.NextDouble()) * chunkSize);
                        } while (tries > 0 && (!CanMove(x, y) || map[x, y].regionId != -1));
                        if (tries == 0) continue;
                        map[x, y].regionId = numReg++;
                        MapRegion reg = new MapRegion();
                        reg.spawnPoint = new Vector2(x, y);
                        reg.links = new Dictionary<int, List<Vector2>>();
                        regionMap.Add(reg);
                        regions.Enqueue(reg.spawnPoint);
                    }
                }

                while (regions.Count != 0)
                {
                    Vector2 cur = regions.Dequeue();
                    int id = map[(int)cur.X, (int)cur.Y].regionId;
                    for (int j = -1; j <= 1; ++j)
                        for (int k = -1; k <= 1; ++k)
                        {
                            if (j == 0 && k == 0) continue;
                            Vector2 newPos = cur + new Vector2(j, k);
                            if (!CanMove((int)newPos.X, (int)newPos.Y)) continue; //If outside of bounds or water, ignore and don't expand.

                            int nId = map[(int)newPos.X, (int)newPos.Y].regionId;
                            if (nId == id) continue;

                            if (nId != -1) //If the new tile is already in another region
                            {
                                if (!regionMap[id].links.ContainsKey(nId)) regionMap[id].links.Add(nId, new List<Vector2>());
                                if (!regionMap[nId].links.ContainsKey(id)) regionMap[nId].links.Add(id, new List<Vector2>());

                                List<Vector2> links2 = regionMap[nId].links[id];
                                if (links2.Find((x) => { return x == newPos; }) == default(Vector2))
                                    links2.Add(newPos);
                            }
                            else
                            {
                                map[(int)newPos.X, (int)newPos.Y].regionId = id;
                                regions.Enqueue(newPos);
                            }
                        }
                }
            }

        }

        /// <summary>
        /// Checks if target coordinates are available in the map
        /// </summary>
        /// <returns>True if it is within position is available</returns>
        public bool CanMove(int x, int y, Creature.HeightLayer z = Creature.HeightLayer.Ground)
        {
            if (!(x >= 0 && x < mapSize && y >= 0 && y < mapSize) || (z != Creature.HeightLayer.Air && map[x, y].isWater))
                return false;
            if (z == Creature.HeightLayer.Ground || z == Creature.HeightLayer.Air) return true;
            return IsTree(x, y);
        }

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <returns>True if it is within bounds</returns>
        public bool CanMove(Vector3 pos)
        {
            return CanMove((int)pos.X, (int)pos.Y, (Creature.HeightLayer)pos.Z);
        }

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if it is within bounds</returns>
        public bool CheckBounds(int x, int y)
        {
            return (x >= 0 && x < mapSize && y >= 0 && y < mapSize);
        }

        public bool IsTree(int x, int y)
        {
            if (x < 0 || y < 0)
                return false;
            Plant p = map[x, y].plant;
            return (p is Tree || p is EdibleTree);
        }

        #region EntitiesManagement

        /// <summary>
        /// Performs a step of the simulation.
        /// </summary>
        /// <returns>True if ther are any remaining creatures</returns>
        public bool Tick()
        {
            CycleDayNight();
            bool entitiesLeft = EntitiesTick();
            CurrentTick++;
            return entitiesLeft;
        }

        public Creature GetCreature(int creatureID)
        {
            if (!Creatures.ContainsKey(creatureID)) return null;
            return Creatures[creatureID];
        }

        public StaticEntity GetStaticEntity(int creatureID)
        {
            if (!StaticEntities.ContainsKey(creatureID)) return null;
            return StaticEntities[creatureID];
        }

        /// <summary>
        /// Creates a creature in the world.
        /// Creatures are entities with abilities and 'complex' behaviours.
        /// T: Any subclass of Creature i.e. Animal
        /// </summary>
        public T CreateCreature<T>(int x, int y, CreatureChromosome chromosome = null, string name = "None", int fatherID = -1, int motherID = -1) where T : Creature, new()
        {
            T ent = new T();

            ent.Init(entitiesID, this, x, y, chromosome, name, fatherID, motherID);
            
            taxonomy.AddCreatureToSpecies(ent);

#if TRACKER_ENABLED
            if(fatherID != -1)
                ent.BirthEventTrack();
#endif
            entityMap[x, y].Add(ent);

            Creatures.Add(entitiesID, ent);
#if DEBUG
            Console.WriteLine("CREATURE HAS BORN AT " + x + ", " + y + " WITH ID: " + entitiesID);
#endif
            entitiesID++;
            return ent;
        }

        /// <summary>
        /// When a creature is born during the simulation it could happens that 
        /// the parents are not the same species, the father's species could be the mother's progenitor species
        /// or vice versa. If this is the case, we need to decide which one is the most similar to the child to set the species name
        /// </summary>
        /// <returns> Return the name of the child species</returns>
        public string GiveName(CreatureChromosome childChromosome, Creature father, Creature mother)
        {
            return taxonomy.MostSimilarityParent(childChromosome, father, mother);
        }

        /// <summary>
        /// Creates a stable entity in the world.
        /// StableEntities are enitities that do not have complex behaviours,
        /// and fulfill the same objecive during all their life-time.
        /// T: Any subclass of StableEntites i.e. Plant, Corpse
        /// </summary>
        public T CreateStaticEntity<T>(int x, int y, int hp) where T : StaticEntity, new()
        {
            T ent = new T();
            ent.Init(entitiesID, this, x, y, hp);
            StaticEntities.Add(entitiesID, ent);
            entityMap[x, y].Add(ent);

            entitiesID++;
            return ent;
        }

        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Destroy(int entityID)
        {
            IEntity ent;
            if (Creatures.ContainsKey(entityID))
            {
                ent = Creatures[entityID];
                taxonomy.RemoveCreatureToSpecies(Creatures[entityID]);
                Creatures[entityID] = null;
            }
            else
            {
                ent = StaticEntities[entityID];
                StaticEntities[entityID] = null;
            }

            entityMap[ent.x, ent.y].Remove(ent);
            entitiesToDelete.Add(entityID);

        }

        /// <summary>
        /// Steps up wolds time and notifies every creature when 
        /// day or night starts.
        /// </summary>
        private void CycleDayNight()
        {
            bool prevState = IsDaytime;
            int currDayTick = CurrentTick % (ticksHour * hoursDay);
            IsDaytime = currDayTick >= dawnHour && currDayTick <= nightFallHour;

            if (IsDaytime != prevState)
                foreach (Creature c in Creatures.Values)
                    c.CycleDayNight();
        }

        /// <summary>
        /// Performs a tick of the simulation of every entity.
        /// Deletes all entities that need to be destroyed after the tick
        /// </summary>
        /// <returns>True if ther are any remaining creatures</returns>
        private bool EntitiesTick()
        {
            // Tick for every creature, ordered by metabolism
            List<Creature> sortedCreatures = new List<Creature>(Creatures.Values);
            sortedCreatures.Sort(metabolismComparer);
            sortedCreatures.ForEach(delegate (Creature e) { e.Tick(); });
            List<StaticEntity> staticEntitiesToStopUpdating = new List<StaticEntity>();
            // Tick for every static entity
            foreach (StaticEntity sEnt in StaticEntitiesToUpdate)
                if (sEnt.Tick())
                    staticEntitiesToStopUpdating.Add(sEnt);

            foreach (StaticEntity del in staticEntitiesToStopUpdating)
                StaticEntitiesToUpdate.Remove(del);

            // Entity deletion
            entitiesToDelete.ForEach(delegate (int id)
            {
                if (Creatures.ContainsKey(id))
                    Creatures.Remove(id);
                else
                    StaticEntities.Remove(id);
            }
            );
            entitiesToDelete.Clear();

            return sortedCreatures.Count > 0;
        }

        /// <summary>
        /// Returns the creatures in an area with a determined radius.
        /// </summary>
        /// <param name="c">The creature that is perceiving</param>
        public List<Creature> PerceiveCreatures(int cID, int radius)
        {
            List<Creature> results = new List<Creature>();

            if (!Creatures.ContainsKey(cID)) return results;

            Creature c = Creatures[cID];
            for (int i = c.x - radius; i < c.x + radius; i++)
            {
                if (i < 0 || i >= map.GetLength(0)) continue;
                for (int j = c.y - radius; j < c.y + radius; j++)
                {
                    if (j < 0 || j >= map.GetLength(1)) continue;

                    for (int k = 0; k < entityMap[i, j].Count; k++)
                    {
                        if (entityMap[i, j][k] is Creature)
                            results.Add(entityMap[i, j][k] as Creature);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns the entities in an area with a determined radius.
        /// </summary>
        /// <param name="cID">The creature's ID that is perceiving</param>
        /// <param name="radius">The radius to perceive</param>
        public List<StaticEntity> PerceiveEntities(int cID, int radius)
        {
            List<StaticEntity> results = new List<StaticEntity>();
            if (!Creatures.ContainsKey(cID)) return results;

            Creature c = Creatures[cID];
            for (int i = c.x - radius; i < c.x + radius; i++)
            {
                if (i < 0 || i >= map.GetLength(0)) continue;
                for (int j = c.y - radius; j < c.y + radius; j++)
                {
                    if (j < 0 || j >= map.GetLength(1)) continue;
                    if (map[i, j].plant != null)
                        results.Add(map[i, j].plant);

                    for (int k = 0; k < entityMap[i, j].Count; k++)
                    {
                        if (entityMap[i, j][k] is StaticEntity)
                            results.Add(entityMap[i, j][k] as StaticEntity);
                    }
                }
            }

            return results;
        }

        static int entitiesID = 0;   
        public Dictionary<int, Creature> Creatures { get; private set; }
        Comparer<Creature> metabolismComparer;

        public Dictionary<int, StaticEntity> StaticEntities { get; private set; }
        public List<StaticEntity> StaticEntitiesToUpdate;
        List<int> entitiesToDelete;

#endregion

#region Procedural Generation

        public Wave[] heightWaves;      // Passes performed by the Perlin noise and added to the height
        public Wave[] humidityWaves;    // Passes performed by the Perlin noise and added to the humidity
        public Wave[] temperatureWaves; // Passes performed by the Perlin noise and added to the temperature
        bool modifiedHeight = true;
        Func<double, double> evaluateHeight;
        Func<int, int, double> complexEvaluateHeight;
        Func<double, double> evaluateInfluence;
        Func<double, double, double> evaluateFlora;
        Func<double, double, double, double> temperatureSoftener;
        Func<double, int> floraSelector;
        float[,] heightMap, humidityMap, temperatureMap;
        bool complexGen = false;
        int numPasses;

        /// <summary>
        /// Function used to insert further noise into the Perlin noise
        /// </summary>
        public class Wave
        {
            public float seed;
            public float frequency;
            public float amplitude;
        }

        /// <summary>
        /// Gnerates a noise map using Perlin noise
        /// </summary>
        /// <param name="mapDepth">Map size in the Z axis</param>
        /// <param name="mapWidth">Map size in the X axis</param>
        /// <param name="scale">Noise scale</param>
        /// <param name="waves">Waves used to insert further noise</param>
        /// <returns>Generated noise map</returns>
        public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ, Wave[] waves)
        {
            float[,] noiseMap = new float[mapDepth, mapWidth];

            for (int zIndex = 0; zIndex < mapDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < mapWidth; xIndex++)
                {
                    float sampleX = (xIndex + offsetX) / scale;
                    float sampleZ = (zIndex + offsetZ) / scale;

                    float noise = 0f;
                    float normalization = 0f;
                    foreach (Wave wave in waves)
                    {
                        noise += (float)(wave.amplitude * p.perlin(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed, 0));
                        normalization += wave.amplitude;
                    }
                    noise /= normalization;

                    noiseMap[zIndex, xIndex] = noise;
                }
            }

            return noiseMap;
        }

        /// <summary>
        /// Processes the values of the maps.
        /// These waves influence each other following certain formulas.
        /// </summary>
        /// <returns>Generated physical map</returns>
        private void ProcessMapValues(float[,] heightMap, float[,] humidityMap, float[,] temperatureMap, float mapScale)
        {
            int sizeX = heightMap.GetLength(0);
            int sizeY = heightMap.GetLength(1);
            map = new MapData[sizeX, sizeY];
            double avgTemp = 0, avgHumidity = 0;
            int land = 0;
            for (int yIndex = 0; yIndex < sizeY; yIndex++)
            {
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {
                    map[xIndex, yIndex] = new MapData();
                    entityMap[xIndex, yIndex] = new List<IEntity>();
                    if (complexGen)
                    {
                        if (modifiedHeight)
                            map[xIndex, yIndex].height = complexEvaluateHeight(xIndex, yIndex);
                        else map[xIndex, yIndex].height = heightMap[xIndex, yIndex];
                    }
                    else
                    {
                        if (modifiedHeight)
                            map[xIndex, yIndex].height = evaluateHeight(heightMap[xIndex, yIndex]);
                        else map[xIndex, yIndex].height = heightMap[xIndex, yIndex];
                    }
                    if (map[xIndex, yIndex].height > 1) map[xIndex, yIndex].height = 1;
                    if (map[xIndex, yIndex].height >= 0.5) land++;

                    double evaluation = evaluateInfluence(map[xIndex, yIndex].height);
                    if (evaluation >= 0)
                    {
                        map[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex] - evaluation;
                        avgTemp -= evaluation;
                        map[xIndex, yIndex].humidity += humidityMap[xIndex, yIndex] + evaluation;
                        if (map[xIndex, yIndex].temperature < 0) map[xIndex, yIndex].temperature = 0; // So it does not excede the domain
                        if (map[xIndex, yIndex].humidity > 1f) map[xIndex, yIndex].humidity = 1f;
                    }
                    else
                    {
                        map[xIndex, yIndex].humidity = humidityMap[xIndex, yIndex];
                        map[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex];
                    }
                    avgTemp += map[xIndex, yIndex].temperature;
                    avgHumidity += map[xIndex, yIndex].humidity;
                }
            }

            double currAvgTemp = avgTemp / Math.Pow(mapSize, 2);
            for (int yIndex = 0; yIndex < sizeY; yIndex++)
            {
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {
                    double evaluation = evaluateInfluence(map[xIndex, yIndex].height);
                    if (evaluation < 0)
                    {
                        avgTemp -= map[xIndex, yIndex].temperature;
                        avgHumidity -= map[xIndex, yIndex].humidity;
                        map[xIndex, yIndex].temperature = temperatureSoftener(evaluation, map[xIndex, yIndex].temperature, currAvgTemp);
                        map[xIndex, yIndex].humidity += map[xIndex, yIndex].humidity - evaluation;
                        map[xIndex, yIndex].isWater = true;
                        for (int i = -(int)(mapScale / 5); i <= (int)(mapScale / 5); i++)
                        {
                            for (int j = -(int)(mapScale / 5); j <= (int)(mapScale / 5); j++)
                            {
                                if (j == 0 && i == 0) continue;
                                if (CheckBounds(xIndex + i, yIndex + j))
                                {
                                    avgHumidity -= map[xIndex + i, yIndex + j].humidity;
                                    avgTemp -= map[xIndex + i, yIndex + j].temperature;

                                    map[xIndex + i, yIndex + j].temperature = temperatureSoftener(evaluation / (1 * (Math.Sqrt(i * i + j * j))), map[xIndex + i, yIndex + j].temperature, currAvgTemp);
                                    map[xIndex + i, yIndex + j].humidity += (-evaluation) / (20 * (Math.Sqrt(i * i + j * j)));
                                    if (map[xIndex + i, yIndex + j].humidity > 1f) map[xIndex + i, yIndex + j].humidity = 1f;
                                    if (map[xIndex + i, yIndex + j].temperature > 1f) map[xIndex + i, yIndex + j].temperature = 1f;

                                    avgHumidity += map[xIndex + i, yIndex + j].humidity;
                                    avgTemp += map[xIndex + i, yIndex + j].temperature;
                                }
                            }
                        }

                        if (map[xIndex, yIndex].humidity > 1f) map[xIndex, yIndex].humidity = 1f;
                        if (map[xIndex, yIndex].temperature > 1f) map[xIndex, yIndex].temperature = 1f;

                        avgHumidity += map[xIndex, yIndex].humidity;
                        avgTemp += map[xIndex, yIndex].temperature;
                    }
                }
            }

            int trees = 0;
            int maxTrees = 0;
            int maxFlora = 0;
            double avgFlora = 0;
            for (int yIndex = 0; yIndex < sizeY; yIndex++)
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {

                    if (map[xIndex, yIndex].height >= 0.5f)
                        map[xIndex, yIndex].flora = evaluateFlora(map[xIndex, yIndex].humidity, map[xIndex, yIndex].temperature);
                    else
                        map[xIndex, yIndex].flora = 0;

                    double val = map[xIndex, yIndex].flora;
                    avgFlora += val;
                    if (val >= 0 && RandomGenerator.NextDouble() <= val)
                    {
                        int plantType = floraSelector(val);
                        maxFlora++;
                        switch (plantType)
                        {
                            case 0:
                                map[xIndex, yIndex].plant = CreateStaticEntity<Grass>(xIndex, yIndex, UniverseParametersManager.parameters.grassHp);
                                EdiblePlants++;
                                break;
                            case 1:
                                map[xIndex, yIndex].plant = CreateStaticEntity<Bush>(xIndex, yIndex, UniverseParametersManager.parameters.bushHp);
                                EdiblePlants++;
                                break;
                            case 2:
                                maxTrees++;
                                map[xIndex, yIndex].plant = CreateStaticEntity<Tree>(xIndex, yIndex, 0);
                                break;
                            case 3:
                                maxTrees++;
                                trees++;
                                map[xIndex, yIndex].plant = CreateStaticEntity<EdibleTree>(xIndex, yIndex, UniverseParametersManager.parameters.eTreeHp);
                                EdiblePlants++;
                                break;
                            default:
                                break;
                        }
                    }
                }
#if DEBUG
            Console.WriteLine("Walkable Area: " + Math.Truncate(((float)land * 100f / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("Average Temperature: " + Math.Truncate((avgTemp * 100 / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("Average Humidity: " + Math.Truncate((avgHumidity * 100 / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("Average Flora: " + Math.Truncate((avgFlora * 100 / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("EdibleTrees / Total Trees: " + Math.Truncate(((float)trees * 100f / maxTrees) * 100) / 100 + " %");
            Console.WriteLine("EdibleTrees / Map: " + Math.Truncate(((float)trees * 100f / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("Total Trees / Map: " + Math.Truncate(((float)maxTrees * 100f / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
            Console.WriteLine("Total Flora / Map: " + Math.Truncate(((float)maxFlora * 100f / Math.Pow(mapSize, 2)) * 100) / 100 + " %");
#endif
        }

        /// <summary>
        /// Gneerates the whole map randomly
        /// </summary>
        public void InitMap()
        {
            float mapScale = 50;
            int tileDepth = mapSize;
            int tileWidth = mapSize;

            float offsetX = 0;
            float offsetZ = 0;

            if (heightMap == null) heightMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, heightWaves);
            if (humidityMap == null) humidityMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, humidityWaves);
            if (temperatureMap == null) temperatureMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, temperatureWaves);

            ProcessMapValues(heightMap, humidityMap, temperatureMap, mapScale);
        }

        double EvaluateFloraCurve(double humidity, double temperature)
        {
            double a = 1, c = 1, d = 1, e = 1.75, g = 0, p = 0;
            double data = ((a / c) * Math.Pow(c * humidity - temperature * p, e) - Math.Pow(2 * temperature - 1 - humidity * g, 2 * d));
            return Math.Min(Math.Max(data, 0), 1.0f);
            //return 2 * Math.Pow((0.5f * (-2 * (Math.Pow(Math.Sqrt(2) * ((mapData[xIndex, yIndex].temperature) - 0.5), 2)) + 1) + (0.5f * mapData[xIndex, yIndex].humidity)), 4);
        }

        double SoftenTemperatureByHeight(double height, double temp, double avg)
        {
            double avgTemp = avg, a = 1;
            return Math.Pow((1 + height), a) * (temp - avgTemp) + avgTemp;
        }

        /// <summary>
        /// Evaluatues a value according to the height function
        /// </summary>
        double DefaultEvaluateHeightCurve(double x)
        {
            //A sets the end of the first slope
            //B sets the height where A stops
            //C1 sets the start of the second slope
            double a = 0.3, b = 0.5, c1 = 0.6, b1 = 1 - b, a1 = 1 - c1, d1 = b;
            if (x < 0) return 0f;
            else if (x < a) return (b / 2) * Math.Sin((Math.PI / a) * (x - a / 2)) + b / 2;
            else if (x < c1) return b;
            else if (x < 1) return (b1 / 2) * Math.Sin((Math.PI / a1) * (x - a1 / 2 - c1)) + b1 / 2 + d1;
            else return 1;
        }

        double IslandEvaluateHeightCurve(int xIndex, int yIndex)
        {
            double e = heightMap[xIndex, yIndex];
            double nx = (2f * xIndex / map.GetLength(0)) - 1f, ny = (2f * yIndex / map.GetLength(1)) - 1f, d = 1f - (1f - nx * nx) * (1f - ny * ny);
            return (1 + e - d) / 2f;
        }

        double AtollEvaluateHeightCurve(int xIndex, int yIndex)
        {
            double e = heightMap[xIndex, yIndex];
            double nx = (2f * xIndex / map.GetLength(0)) - 1f, ny = (2f * yIndex / map.GetLength(1)) - 1f;
            return e * Math.Sin(3 * (nx * nx + ny * ny)) * 1.25f;
        }

        /// <summary>
        /// Evaluatues a value according to the influence function
        /// </summary>
        double EvaluateInfluenceCurve(double x)
        {
            if (x < 0) return -1;
            else if (x < 0.5) return Math.Pow((2 * x), 2) - 1;
            else if (x < 1) return 2 * (x - 0.5);
            else return 1;
        }

        int ChoosePlant(double floraProb)
        {
            double treeThreshold = 0.4, bushThreshold = 0.2;
            if (floraProb <= treeThreshold)
            {
                if (RandomGenerator.NextDouble() <= ((1 / (treeThreshold - bushThreshold)) * (floraProb - bushThreshold)))
                    return 1;
                else
                    return 0;
            }
            else
            {
                if (RandomGenerator.NextDouble() < 0.95)
                    return 2;
                else
                    return 3;
            }
        }
#endregion

        /// <summary>
        /// Given a year, returns the number of ticks it equals
        /// </summary>
        public int YearToTick(float year)
        {
            return (int)(year * daysYear * hoursDay * ticksHour);
        }

        /// <summary>
        /// Get the number of existing species 
        /// </summary>
        public int GetSpeciesNumber()
        {
            return taxonomy.GetSpeciesNumber();
        }

        /// <summary>
        /// Check if 2 species are related
        /// they are related only if they are the same species, they have the same progenitor
        /// or one is the progenitor or grandprogenitor of the other
        /// </summary>
        /// <returns>return true if they are related</returns>
        public bool AreRelated(string speciesName1, string speciesName2)
        {
            return taxonomy.AreRelated(speciesName1, speciesName2);
        }

        /// <summary>
        /// Export the content information when the simulation ends
        /// </summary>
        public void ExportContent()
        {
            string path;
#if TRACKER_ENABLED
            path = $"{UserInfo.ExportDirectory}Output/{Telemetry.Tracker.Instance.SessionID}";
#else
            path = $"{UserInfo.ExportDirectory}Output";

#endif
            System.IO.Directory.CreateDirectory(path);
            taxonomy.ExportSpecies(path);
            taxonomy.RenderSpeciesTree($"{path}/{UserInfo.TreeName}");
            string word = JsonConvert.SerializeObject(map, Formatting.Indented);
            System.IO.File.WriteAllText($"{path}/{UserInfo.WorldName}", word);
            string rMap = JsonConvert.SerializeObject(regionMap, Formatting.Indented);
            System.IO.File.WriteAllText($"{path}/{UserInfo.RegionMapName}", rMap);
        }

        // Map with physical properties
        public MapData[,] map { get; private set; }

        public List<IEntity>[,] entityMap;
        public List<MapRegion> regionMap { get; private set; }
        int mapSize;
        public int chunkSize { get; private set; }
        // Perlin noise generator
        Perlin p;

        public static int ticksHour, hoursDay, daysYear;

        /// <summary>
        /// The current tick associated with the state of the world in the simulation
        /// </summary>
        public int CurrentTick { get; private set; }

        /// <summary>
        /// Boolean that tells if it its daytime (True) or nighttime (False)
        /// </summary>
        public bool IsDaytime { get; private set; }

        /// <summary>
        /// Variables to handle the night/day cycle
        /// </summary>
        float dawnHour, nightFallHour;

        /// <summary>
        /// Species taxonomy manager
        /// </summary>
        GeneticTaxonomy taxonomy;
    }
}
