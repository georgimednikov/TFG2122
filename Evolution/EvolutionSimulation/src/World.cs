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
    }

    /// <summary>
    /// Definition of the simulated world
    /// </summary>
    public class World
    {
        /// <summary>
        /// Properties of each map tile
        /// </summary>
        public struct MapData
        {
            public double height, humidity, temperature, flora;
            public Plant plant;
            public bool isWater;
        }

        public void Init(int size)
        {
            WorldGenConfig c = new WorldGenConfig();
            c.mapSize = size;
            Init(c);
        }

        /// <summary>
        /// Initializes the map with a matrix of provided size.
        /// </summary>
        public void Init(WorldGenConfig config)
        {
            if (config == null) throw new NullReferenceException("World generation config is null");

            Validator.ValidatorResult result;
            if ((result = Validator.Validate(config)) != Validator.ValidatorResult.NoError) Validator.ExceptionThrow(result); //TODO: Preguntar a Cleon si hay una manera mejor

            evaluateHeight = (config.evaluateHeight != null) ? config.evaluateHeight : EvaluateHeightCurve;
            evaluateInfluence = (config.evaluateInfluence != null) ? config.evaluateInfluence : EvaluateInfluenceCurve;
            evaluateFlora = (config.evaluateFlora != null) ? config.evaluateFlora : EvaluateFloraCurve;
            temperatureSoftener = (config.temperatureSoftener != null) ? config.temperatureSoftener : SoftenTemperatureByHeight;
            floraSelector = (config.floraSelector != null) ? config.floraSelector : ChoosePlant;
            modifiedHeight = config.heightModifiedByFunction;
            taxonomy = new GeneticTaxonomy();
            taxonomy.Init();
            Creatures = new List<Creature>();
            StableEntities = new List<StaticEntity>();
            CreaturesToDelete = new List<IEntity>();
            SEntitiesToDelete = new List<IEntity>();
            p = new Perlin();
            if (config.heightMap != null) { heightMap = config.heightMap; mapSize = heightMap.GetLength(0); }
            else mapSize = config.mapSize;

            if (config.heightWaves != null) heightWaves = config.heightWaves;
            else
            {
                heightWaves = new Wave[2];
                heightWaves[0] = new Wave();
                heightWaves[0].seed = RandomGenerator.Next(0, 10000);
                heightWaves[0].frequency = 0.5f;
                heightWaves[0].amplitude = 1f;
                heightWaves[1] = new Wave();
                heightWaves[1].seed = RandomGenerator.Next(0, 10000);
                heightWaves[1].frequency = 1f;
                heightWaves[1].amplitude = 0;
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

            InitMap();
        }

        /// <summary>
        /// Checks if target coordinates are available in the map
        /// </summary>
        /// <returns>True if it is within position is available</returns>
        public bool canMove(int x, int y, Creature.HeightLayer z = Creature.HeightLayer.Ground)
        {
            if (!(x >= 0 && x < mapSize && y >= 0 && y < mapSize) || (z != Creature.HeightLayer.Air && map[x, y].isWater)) return false;
            if (z == Creature.HeightLayer.Ground || z == Creature.HeightLayer.Air) return true;
            return isTree(x, y);
        }

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if it is within bounds</returns>
        public bool checkBounds(int x, int y)
        {
            return (x >= 0 && x < mapSize && y >= 0 && y < mapSize);
        }

        public bool isTree(int x, int y)
        {
            Plant p = map[x, y].plant;
            return (p is Tree || p is EdibleTree);
        }

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <returns>True if it is within bounds</returns>
        public bool canMove(Vector3 pos)
        {
            return canMove((int)pos.X, (int)pos.Y, (Creature.HeightLayer)pos.Z);
        }

        #region EntitiesManagement
        /// <summary>
        /// Performs a step of the simulation.
        /// </summary>
        public void Tick()
        {
            EntitiesTick();
            step++;
            day = (step % (ticksHour * hoursDay) >= (morning * ticksHour) &&
                step % (ticksHour * hoursDay) <= (night * ticksHour));
        }


        /// <summary>
        /// Creates a creature in the world.
        /// Creatures are entities with abilities and  'complex' behaviours.
        /// T: Any subclass of Creature i.e. Animal
        /// </summary>
        public T CreateCreature<T>(int x, int y, CreatureChromosome chromosome = null, string name = "None") where T : Creature, new()
        {
            T ent = CreateEntity<T>();
            Creatures.Add(ent);
            ent.Init(this, x, y, chromosome, name);
            taxonomy.AddCreatureToSpecies(ent);

            return ent;
        }

        /// <summary>
        /// Creates a stable entity in the world.
        /// StableEntities are enitities that do not have complex behaviours,
        /// and fulfill the same objecive during all their life-time.
        /// T: Any subclass of StableEntites i.e. Plant, Corpse
        /// </summary>
        public T CreateStableEntity<T>() where T : StaticEntity, new()
        {
            T ent = CreateEntity<T>();
            StableEntities.Add(ent);
            return ent;
        }
        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Destroy(Creature entity)
        {
            CreaturesToDelete.Add(entity);
        }

        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Destroy(StaticEntity entity)
        {
            SEntitiesToDelete.Add(entity);
        }

        /// <summary>
        /// Performs a tick of the simulation of every entity.
        /// Deletes all entities that need to be destroyed after the tick
        /// </summary>
        private void EntitiesTick()
        {
            // Tick for every entity
            Creatures.Sort(new Utils.SortByMetabolism()); // TODO: priority queue
            Creatures.ForEach(delegate (Creature e) { e.Tick(); });
            StableEntities.ForEach(delegate (StaticEntity e) { e.Tick(); });

            // Entity deletion
            CreaturesToDelete.ForEach(delegate (IEntity e)
            {
                Creatures.Remove(e as Creature);
                e = null;
                //foreach (Creature c in Creatures)
                //    if (c.objective == e) c.objective = null;   // TODO URGENTE: Esto no deberia hacerse, pero ni poniendolo en null se quita la referencia al objetivo de la criatura
            });
            SEntitiesToDelete.ForEach(delegate (IEntity e)
            {
                StableEntities.Remove(e as StaticEntity);
                e = null;
                //foreach (Creature c in Creatures)
                //    if (c.objective == e) c.objective = null;
            });
            CreaturesToDelete.Clear();
            SEntitiesToDelete.Clear();
        }

        /// <summary>
        /// Adds an entity to the list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>The added entity</returns>
        T CreateEntity<T>() where T : IEntity, new()
        {
            return new T();

        }

        /// <summary>
        /// Returns the creatures in an area with a determined radius.
        /// </summary>
        /// <param name="c">The creature that is perceiving</param>
        public List<Creature> PerceiveCreatures(Creature c, int radius)
        {
            List<Creature> results = new List<Creature>();
            foreach (Creature e in Creatures) // TODO: use this?
            {
                if (e == c) continue; // Reference comparison
                if (Math.Abs(e.x - c.x) <= radius && Math.Abs(e.y - c.y) <= radius) // Square vision
                    results.Add(e);
            }
            return results;
        }

        /// <summary>
        /// Returns the entities in an area with a determined radius.
        /// </summary>
        /// <param name="c">The creature that is perceiving</param>
        public List<StaticEntity> PerceiveEntities(Creature c, int radius)
        {
            List<StaticEntity> results = new List<StaticEntity>();
            foreach (StaticEntity e in StableEntities) // TODO: use this?
            {
                if (Math.Abs(e.x - c.x) <= radius && Math.Abs(e.y - c.y) <= radius) // Square vision
                    results.Add(e);
            }
            return results;
        }
        #endregion

        #region Procedural Generation

        public Wave[] heightWaves;      // Passes performed by the Perlin noise and added to the height
        public Wave[] humidityWaves;    // Passes performed by the Perlin noise and added to the humidity
        public Wave[] temperatureWaves; // Passes performed by the Perlin noise and added to the temperature
        bool modifiedHeight = true;
        Func<double, double> evaluateHeight;
        Func<double, double> evaluateInfluence;
        Func<double, double, double> evaluateFlora;
        Func<double, double, double, double> temperatureSoftener;
        Func<double, int> floraSelector;
        float[,] heightMap, humidityMap, temperatureMap;

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
            for (int yIndex = 0; yIndex < sizeY; yIndex++)
            {
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {
                    map[xIndex, yIndex].height = modifiedHeight ? evaluateHeight(heightMap[xIndex, yIndex]) : heightMap[xIndex, yIndex];

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
                                if (checkBounds(xIndex + i, yIndex + j))
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
                                map[xIndex, yIndex].plant = new Grass();
                                break;
                            case 1:
                                map[xIndex, yIndex].plant = new Bush();
                                break;
                            case 2:
                                maxTrees++;
                                map[xIndex, yIndex].plant = new Tree();
                                break;
                            case 3:
                                maxTrees++;
                                trees++;
                                map[xIndex, yIndex].plant = new EdibleTree();
                                break;
                            default:
                                break;
                        }
                    }
                }
#if DEBUG
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
        double EvaluateHeightCurve(double x)
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
        /// Guiven a year, returns the number of ticks it equals
        /// </summary>
        public int YearToTick(float year)
        {
            return (int)(year * daysYear * hoursDay * ticksHour);
        }

        public void ExportContent()
        {
            taxonomy.ExportSpecies();
            string word = JsonConvert.SerializeObject(map, Formatting.Indented);
            System.IO.File.WriteAllText(UserInfo.ExportDirectory + "World.json", word);
        }

        // Map with physical properties
        public MapData[,] map { get; private set; }
        int mapSize;
        bool day;
        public uint step;
        // 50 steps equals and hour, and 24 hours equal a day. 365 days equal a year
        int ticksHour = 50, hoursDay = 24, daysYear = 365;  // TODO: Quitar lo de year, es necesario?
        // The day begins 6:30 and ends at 20:00.
        float morning = 6.5f, night = 20;
        // Perlin noise generator
        Perlin p;

        // Entities management
        public List<Creature> Creatures { get; private set; }
        public List<StaticEntity> StableEntities { get; private set; }
        GeneticTaxonomy taxonomy;


        // TODO: podemos dejar esto asi o comparar los tipos en una sola lista
        List<IEntity> CreaturesToDelete;
        List<IEntity> SEntitiesToDelete;
    }
}
