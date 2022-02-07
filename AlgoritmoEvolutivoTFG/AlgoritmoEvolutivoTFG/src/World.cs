using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
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
        }

        /// <summary>
        /// Initializes the map with a square matrix
        /// </summary>
        public void Init(int size)
        {
            p = new Perlin();
            mapSize = size;
            entities = new List<IEntity>();
            delete = new List<IEntity>();
            heightWaves = new Wave[1];
            heightWaves[0] = new Wave();
            heightWaves[0].seed = 5645;
            heightWaves[0].frequency = 1f;
            heightWaves[0].amplitude = 1f;
            humidityWaves = new Wave[1];
            humidityWaves[0] = new Wave();
            humidityWaves[0].seed = 4515;
            humidityWaves[0].frequency = 0.5f;
            humidityWaves[0].amplitude = 1f;
            temperatureWaves = new Wave[1];
            temperatureWaves[0] = new Wave();
            temperatureWaves[0].seed = 516;
            temperatureWaves[0].frequency = 0.05f;
            temperatureWaves[0].amplitude = 1f;
            Init();
        }

        /// <summary>
        /// Adds an entity to the list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>The added entity</returns>
        public T AddEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            entities.Add(ent);
            return ent;
        }

        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }

        /// <summary>
        /// Checks if target coordinates are within the map's boundaries
        /// </summary>
        /// <returns>True if it is within bounds</returns>
        public bool canMove(int x, int y)
        {
            return (x >= 0 && x < mapSize && y >= 0 && y < mapSize);
        }

        /// <summary>
        /// Performs a step of the simulation
        /// </summary>
        public void Tick()
        {
            entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Orders the entity to perform a step

            delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

            delete.Clear();
        }

        #region Procedural Generation

        public Wave[] heightWaves;      // Passes performed by the Perlin noise and added to the height
        public Wave[] humidityWaves;    // Passes performed by the Perlin noise and added to the humidity
        public Wave[] temperatureWaves; // Passes performed by the Perlin noise and added to the temperature

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
        private MapData[,] ProcessMapValues(float[,] heightMap, float[,] humidityMap, float[,] temperatureMap)
        {
            int sizeX = heightMap.GetLength(0);
            int sizeY = heightMap.GetLength(1);
            MapData[,] mapData = new MapData[sizeX, sizeY];

            for (int yIndex = 0; yIndex < sizeY; yIndex++)
            {
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {
                    mapData[xIndex, yIndex].height = EvaluateHeightCurve(heightMap[xIndex, yIndex]);

                    double evaluation = EvaluateInfluenceCurve(mapData[xIndex, yIndex].height);
                    if (evaluation > 0) 
                    {
                        mapData[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex] - evaluation / 2;
                        mapData[xIndex, yIndex].humidity = humidityMap[xIndex, yIndex] + evaluation;
                        if (mapData[xIndex, yIndex].temperature < 0) mapData[xIndex, yIndex].temperature = 0; // So it does not excede the domain
                        if (mapData[xIndex, yIndex].humidity > 1f) mapData[xIndex, yIndex].humidity = 1f;
                    }
                    else if (evaluation < 0)
                    {
                        mapData[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex];
                        mapData[xIndex, yIndex].humidity = humidityMap[xIndex, yIndex] - evaluation;

                        if (mapData[xIndex, yIndex].humidity > 1f) mapData[xIndex, yIndex].humidity = 1f;
                    }
                    else
                    {
                        mapData[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex];
                        mapData[xIndex, yIndex].humidity = humidityMap[xIndex, yIndex];
                    }

                    if (mapData[xIndex, yIndex].height >= 0.5f) 
                        mapData[xIndex, yIndex].flora = Math.Min(Math.Max(mapData[xIndex, yIndex].temperature * 2 * mapData[xIndex, yIndex].humidity, 0), 100);
                    else
                        mapData[xIndex, yIndex].flora = 0;

                }
            }
            return mapData;
        }

        /// <summary>
        /// Gneerates the whole map randomly
        /// </summary>
        public void Init()
        {
            float mapScale = 20f;
            int tileDepth = mapSize;
            int tileWidth = mapSize;

            float offsetX = 0;
            float offsetZ = 0;

            float[,] heightMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, heightWaves);
            float[,] humidityMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, humidityWaves);
            float[,] temperatureMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, temperatureWaves);

            map = ProcessMapValues(heightMap, humidityMap, temperatureMap);
        }

        /// <summary>
        /// Evaluatues a value according to the height function
        /// </summary>
        double EvaluateHeightCurve(double x)
        {
            if (x < 0) return 0f;
            else if (x < 0.3) return (1 / 4f) * Math.Sin((10 / 3) * Math.PI * (x - 0.15)) + 0.25f;
            else if (x < 0.5) return 0.5f;
            else if (x < 1) return (1 / 4f) * Math.Sin(2 * Math.PI * (x - 0.75f)) + 0.75f;
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
        #endregion

        // Entities in the world
        public List<IEntity> entities { get; private set; }
        // Entities to be deleted
        List<IEntity> delete;
        // Map with physical properties
        public MapData[,] map{ get; private set; }
        int mapSize;
        // Perlin noise generator
        Perlin p;
    }
}
