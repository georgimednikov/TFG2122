using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
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

        Func<double, double> evaluateHeight;
        Func<double, double> evaluateInfluence;

        /// <summary>
        /// Initializes the map with a square matrix, optionally you can pass additional function to change how the Perlin Noise is interpreted and how much the height influences the rest.
        /// </summary>
        /// <param name="heightFunc">Function to interpret the results from the Perlin Noise function</param>
        /// <param name="influenceFunc">Function that defines the influence of height on the other parameters</param>
        public void Init(int size, Func<double, double> heightFunc = default(Func<double, double>), Func<double, double> influenceFunc = default(Func<double, double>))
        {
            evaluateHeight = (heightFunc != null) ? heightFunc : EvaluateHeightCurve;
            evaluateInfluence = (influenceFunc != null) ? influenceFunc : EvaluateInfluenceCurve;
            p = new Perlin();
            mapSize = size;
            Random rnd = new Random(DateTime.Now.Second);
            heightWaves = new Wave[1];
            heightWaves[0] = new Wave();
            heightWaves[0].seed = rnd.Next(0, 10000); //1641;
            heightWaves[0].frequency = 1f;
            heightWaves[0].amplitude = 1f;
            //heightWaves[1] = new Wave();
            //heightWaves[1].seed = rnd.Next(0, 10000); //1641;
            //heightWaves[1].frequency = 0.5f;
            //heightWaves[1].amplitude = 1f;
            //heightWaves[2] = new Wave();
            //heightWaves[2].seed = rnd.Next(0, 10000); //1641;
            //heightWaves[2].frequency = 0.5f;
            //heightWaves[2].amplitude = 1f;
            humidityWaves = new Wave[1];
            humidityWaves[0] = new Wave();
            humidityWaves[0].seed = rnd.Next(0, 10000);//4534;
            humidityWaves[0].frequency = 0.5f;
            humidityWaves[0].amplitude = 1f;
            temperatureWaves = new Wave[1];
            temperatureWaves[0] = new Wave();
            temperatureWaves[0].seed = rnd.Next(0, 10000);// 453;
            temperatureWaves[0].frequency = 0.25f;
            temperatureWaves[0].amplitude = 1f;
            InitMap();
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
        /// Performs a step of the simulation.
        /// </summary>
        public void Tick()
        {
            step++;
            day = (step % (ticksHour * hoursDay) >= (morning * ticksHour) && 
                step % (ticksHour * hoursDay) <= (night * ticksHour));
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
        private void ProcessMapValues(float[,] heightMap, float[,] humidityMap, float[,] temperatureMap, float mapScale)
        {
            int sizeX = heightMap.GetLength(0);
            int sizeY = heightMap.GetLength(1);
            map = new MapData[sizeX, sizeY];

            for (int yIndex = 0; yIndex < sizeY; yIndex++)
            {
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                {
                    map[xIndex, yIndex].height = evaluateHeight(heightMap[xIndex, yIndex]);

                    double evaluation = evaluateInfluence(map[xIndex, yIndex].height);
                    if (evaluation > 0)
                    {
                        map[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex] - evaluation;
                        map[xIndex, yIndex].humidity += humidityMap[xIndex, yIndex] + evaluation;
                        if (map[xIndex, yIndex].temperature < 0) map[xIndex, yIndex].temperature = 0; // So it does not excede the domain
                        if (map[xIndex, yIndex].humidity > 1f) map[xIndex, yIndex].humidity = 1f;
                    }
                    else if (evaluation < 0)
                    {
                        map[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex];
                        map[xIndex, yIndex].humidity += humidityMap[xIndex, yIndex] - evaluation;
                        for (int i = -(int)(mapScale / 5); i <= (int)(mapScale / 5); i++)
                        {
                            for (int j = -(int)(mapScale / 5); j <= (int)(mapScale / 5); j++)
                            {
                                if (j == 0 && i == 0) continue;
                                if (canMove(xIndex + i, yIndex + j))
                                {
                                    map[xIndex + i, yIndex + j].humidity += (-evaluation) / (20 * (Math.Sqrt(i * i + j * j)));
                                    if (map[xIndex + i, yIndex + j].humidity > 1f) map[xIndex + i, yIndex + j].humidity = 1f;
                                }
                            }
                        }

                        if (map[xIndex, yIndex].humidity > 1f) map[xIndex, yIndex].humidity = 1f;
                    }
                    else
                    {
                        map[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex];
                        map[xIndex, yIndex].humidity += humidityMap[xIndex, yIndex];
                    }
                }
            }

            for (int yIndex = 0; yIndex < sizeY; yIndex++)
                for (int xIndex = 0; xIndex < sizeX; xIndex++)
                    if (map[xIndex, yIndex].height >= 0.5f)
                        map[xIndex, yIndex].flora = EvaluateFloraCurve(xIndex, yIndex);
                    else
                        map[xIndex, yIndex].flora = 0;

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

            float[,] heightMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, heightWaves);
            float[,] humidityMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, humidityWaves);
            float[,] temperatureMap = GenerateNoiseMap(tileDepth, tileWidth, mapScale, offsetX, offsetZ, temperatureWaves);

            ProcessMapValues(heightMap, humidityMap, temperatureMap, mapScale);
        }

        double EvaluateFloraCurve(int xIndex, int yIndex)
        {
            double a = 1, c = 1, d = 1, e = 1.75, g = 0, p = 0;
            double data = ((a / c) * Math.Pow(c * map[xIndex, yIndex].humidity - map[xIndex, yIndex].temperature * p, e) - Math.Pow(2 * map[xIndex, yIndex].temperature - 1 - map[xIndex, yIndex].humidity * g, 2 * d));
            return Math.Min(Math.Max(data, 0), 1.0f);
            //return 2 * Math.Pow((0.5f * (-2 * (Math.Pow(Math.Sqrt(2) * ((mapData[xIndex, yIndex].temperature) - 0.5), 2)) + 1) + (0.5f * mapData[xIndex, yIndex].humidity)), 4);
        }

        /// <summary>
        /// Evaluatues a value according to the height function
        /// </summary>
        double EvaluateHeightCurve(double x)
        {
            if (x < 0) return 0f;
            else if (x < 0.3) return (1 / 4f) * Math.Sin((10f / 3f) * Math.PI * (x - 0.15)) + 0.25f;
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

        public uint step;
        // Map with physical properties
        public MapData[,] map { get; private set; }
        int mapSize;
        bool day;
        // 50 steps equals and hour, and 24 hours equal a day.
        int ticksHour = 50, hoursDay = 24;
        // The day begins 6:30 and ends at 20:00.
        float morning = 6.5f, night = 20;
        // Perlin noise generator
        Perlin p;
    }
}
