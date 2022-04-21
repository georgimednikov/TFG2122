using EvolutionSimulation.Entities;
using EvolutionSimulation.Genetics;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;

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
        public int mapSize;
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
        /// Number of wholly isolated landmasses per chunk (32x32). Increase if you are getting
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
        public int tick { get; private set; }
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

        // TODO: esta constructora se puede ir?
        public void Init(int size)
        {
            WorldGenConfig c = new WorldGenConfig(MapType.Default);
            c.mapSize = size;
            Init(c);
        }

        // TODO: se va a poder inicializar asi o solo con un config?
        public void Init(string rawWorldData, string regionMap)
        {
            WorldGenConfig config = new WorldGenConfig(MapType.Custom);
            map = JsonConvert.DeserializeObject<MapData[,]>(rawWorldData);
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
            highMap = JsonConvert.DeserializeObject<List<MapRegion>>(regionMap);
            config.regionMap = highMap;
            Validator.Validate(config);

            ticksHour = UniverseParametersManager.parameters.ticksPerHour;
            hoursDay = UniverseParametersManager.parameters.hoursPerDay;
            daysYear = UniverseParametersManager.parameters.daysPerYear;
            morning = UniverseParametersManager.parameters.morningStart;
            night = UniverseParametersManager.parameters.nightStart;

            mapSize = map.GetLength(0);
            taxonomy = new GeneticTaxonomy();
            Creatures = new Dictionary<int, Creature>();
            metabolismComparer = new Utils.SortByMetabolism();
            StaticEntities = new Dictionary<int, StaticEntity>();
            entitiesToDelete = new List<int>();

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
                            break;
                        case Plant.PlantType.Bush:
                            plant = CreateStaticEntity<Bush>(i, j, UniverseParametersManager.parameters.bushHp);
                            break;
                        case Plant.PlantType.Grass:
                            plant = CreateStaticEntity<Grass>(i, j, UniverseParametersManager.parameters.grassHp);
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
            morning = UniverseParametersManager.parameters.morningStart;
            night = UniverseParametersManager.parameters.nightStart;

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
            metabolismComparer = new Utils.SortByMetabolism();
            StaticEntities = new Dictionary<int, StaticEntity>();
            entitiesToDelete = new List<int>();
            StaticEntitiesToUpdate = new List<StaticEntity>();

            p = new Perlin();
            if (config.heightMap != null) { heightMap = config.heightMap; mapSize = heightMap.GetLength(0); }
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

            if (config.humidityMap != null) { humidityMap = config.humidityMap; mapSize = humidityMap.GetLength(0); }
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


            if (config.temperatureMap != null) { temperatureMap = config.temperatureMap; mapSize = temperatureMap.GetLength(0); }
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
                int chunk = (int)Math.Floor(mapSize / (float)chunkSize) + ((mapSize % chunkSize == 0) ? 1 : 0);
                highMap = new List<MapRegion>();
                FillHighMap();
            }
            else
            {
                highMap = config.regionMap;
            }
        }

        private void FillHighMap()
        {
            int numReg = 0;
            Queue<Vector2> regions = new Queue<Vector2>(); //TODO: Lista de nodos a encolar 
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
                        highMap.Add(reg);
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
                                if (!highMap[id].links.ContainsKey(nId)) highMap[id].links.Add(nId, new List<Vector2>());
                                if (!highMap[nId].links.ContainsKey(id)) highMap[nId].links.Add(id, new List<Vector2>());

                                List<Vector2> links2 = highMap[nId].links[id];
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

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <returns>True if it is within bounds</returns>
        public bool canMove(Vector3 pos)
        {
            return CanMove((int)pos.X, (int)pos.Y, (Creature.HeightLayer)pos.Z);
        }

        #region EntitiesManagement

        /// <summary>
        /// Performs a step of the simulation.
        /// </summary>
        /// <returns>True if ther are any remaining creatures</returns>
        public bool Tick(int tick)
        {
            this.tick = tick;
            CycleDayNight();
            return EntitiesTick();
        }

        // TODO: que devuelva una criatura no soluciona el problema de la destrucción, a no ser que sea una copia u otro objeto
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
            // Progenitors start being adults and a half is male and the other half female
            if (fatherID == -1)
            {
                ent.stats.CurrAge = (int)(UniverseParametersManager.parameters.adulthoodThreshold * ent.stats.LifeSpan);
                if (Creatures.Count % 2 == 0)
                {
                    ent.chromosome.ModifyGender(Gender.Male);
                    ent.stats.Gender = Gender.Male;
                }
                else
                {
                    ent.chromosome.ModifyGender(Gender.Female);
                    ent.stats.Gender = Gender.Female;
                }
            }
            taxonomy.AddCreatureToSpecies(ent);
            entityMap[x, y].Add(ent);

            Creatures.Add(entitiesID, ent);
#if DEBUG
            Console.WriteLine("CREATURE HAS BORN AT " + x + ", " + y + " WITH ID: " + entitiesID);
#endif
            entitiesID++;
            // TODO: devolver el id, una copia o un wrap del objeto creado
            return ent;
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
                ent = Creatures[entityID];
            else
                ent = StaticEntities[entityID];

            entityMap[ent.x, ent.y].Remove(ent);
            entitiesToDelete.Add(entityID);

        }

        /// <summary>
        /// Steps up wolds time and notifies every creature when 
        /// day or night starts.
        /// </summary>
        private void CycleDayNight()
        {
            bool prevState = day;
            day = step % (ticksHour * hoursDay) >= (morning * ticksHour) && step % (ticksHour * hoursDay) <= (night * ticksHour);

            if (day != prevState)
                foreach (Creature c in Creatures.Values)
                    c.CycleDayNight();
            step++;
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
                {
                    taxonomy.RemoveCreatureToSpecies(Creatures[id]);
                    Creatures.Remove(id);
                }
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
            foreach (Creature e in Creatures.Values)
            {
                if (cID == e.ID) continue;
                if (Math.Abs(e.x - c.x) <= radius && Math.Abs(e.y - c.y) <= radius)// Square vision
                {
                    if (c.speciesName == e.speciesName)
                    {
                        results.Add(e);
                        continue;
                    }
                    float perception = c.stats.Perception / (float)c.stats.MaxPerception;
                    float camouflage = e.stats.Camouflage / (float)e.chromosome.GetFeatureMax(CreatureFeature.Camouflage);
                    // Perceive the creature if your perception percentage is greater than him camouflage percentage
                    if (perception > camouflage)
                        results.Add(e);
                    //TODO queremos que haya una probabilidad de que perciba al otro aunque tenga el camuflaje mayor?
                    //else
                    //{   //a probability to perceive the other creature
                    //    int randMax = (int)((camouflage - perception) * 100);
                    //    if(RandomGenerator.Next(100) > randMax)
                    //        results.Add(e);
                    //}
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
                    if ( j < 0 || j >= map.GetLength(1) ) continue;
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

        static int entitiesID = 0;   // TODO: hacer un id unico para cada entidad con hash?
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
                                break;
                            case 1:
                                map[xIndex, yIndex].plant = CreateStaticEntity<Bush>(xIndex, yIndex, UniverseParametersManager.parameters.bushHp);
                                break;
                            case 2:
                                maxTrees++;
                                map[xIndex, yIndex].plant = CreateStaticEntity<Tree>(xIndex, yIndex, 0);
                                break;
                            case 3:
                                maxTrees++;
                                trees++;
                                map[xIndex, yIndex].plant = CreateStaticEntity<EdibleTree>(xIndex, yIndex, UniverseParametersManager.parameters.eTreeHp);
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
        /// Export the content information when the simulation ends
        /// </summary>
        public void ExportContent()
        {
            taxonomy.ExportSpecies();
            taxonomy.RenderSpeciesTree(UserInfo.ExportDirectory + "Tree.json");
            string word = JsonConvert.SerializeObject(map, Formatting.Indented);
            System.IO.File.WriteAllText(UserInfo.ExportDirectory + "World.json", word);
            string hMap = JsonConvert.SerializeObject(highMap, Formatting.Indented);
            System.IO.File.WriteAllText(UserInfo.ExportDirectory + "HighMap.json", hMap);
        }

        /// <summary>
        /// Export just the especies information when an apocalyse occurs
        /// </summary>
        /// <param name="cont"></param>
        public void ApocalypseExportContent(int cont)
        {
            taxonomy.RenderSpeciesTree(UserInfo.ExportDirectory + "/Apocalyse" + cont + "Tree.txt", tick);
            taxonomy.ExportSpecies(cont);
        }

        // Map with physical properties
        public MapData[,] map { get; private set; }

        public List<IEntity>[,] entityMap;
        public List<MapRegion> highMap { get; private set; }
        int mapSize;
        public int chunkSize { get; private set; }
        public bool day;
        public uint step;
        // 50 steps equals and hour, and 24 hours equal a day. 365 days equal a year
        public static int ticksHour, hoursDay, daysYear;
        // The day begins 6:30 and ends at 20:00.
        float morning, night;
        // Perlin noise generator
        Perlin p;

        GeneticTaxonomy taxonomy;
    }
}
