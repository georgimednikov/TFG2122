using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Clase que define el mundo simulado
    /// </summary>
    public class World
    {
        /// <summary>
        /// Datos fisicos de cada casilla del mapa
        /// </summary>
        public struct MapData
        {
            public double height, humidity, temperature, flora;
        }

        /// <summary>
        /// Inicializa el mapa con una matriz cuadrada
        /// </summary>
        /// <param name="size">Tamanio de la matriz</param>
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
        /// Aniade una entidad a la lista del mundo
        /// </summary>
        /// <typeparam name="T">La entidad a aniadir</typeparam>
        /// <returns>Entidad aniadida</returns>
        public T AddEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            entities.Add(ent);
            return ent;
        }

        /// <summary>
        /// Marca a una entidad para que se elimine en el siguiente frame
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }

        /// <summary>
        /// Comprueba si las coordenadas objetivo se encuentran dentro del mapa
        /// </summary>
        /// <param name="x">Coordenada x</param>
        /// <param name="y">Coordenada y</param>
        /// <returns>Si las coordenadas se encuentran en el mapa</returns>
        public bool canMove(int x, int y)
        {
            return (x >= 0 && x < mapSize && y >= 0 && y < mapSize);
        }

        /// <summary>
        /// Paso de ejecucion de la simulacion del mundo
        /// </summary>
        public void Tick()
        {
            entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Le dice a al entidad que se actualice

            delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

            delete.Clear();
        }

        #region Generacion Procedimental

        public Wave[] heightWaves;      // Pasadas realizadas por el ruido de Perlin y sumadas a la altura
        public Wave[] humidityWaves;    // Pasadas realizadas por el ruido de Perlin y sumadas a la humedad 
        public Wave[] temperatureWaves; // Pasadas realizadas por el ruido de Perlin y sumadas a la temperatura

        /// <summary>
        /// Funcion usada para meter ruido a lo generado originalmente por Perlin
        /// </summary>
        public class Wave
        {
            public float seed;
            public float frequency;
            public float amplitude;
        }

        /// <summary>
        /// Genera un mapa de ruido usando ruido de Perlin
        /// </summary>
        /// <param name="mapDepth">Tamanio del mapa en un eje Z</param>
        /// <param name="mapWidth">Tamanio del mapa en el eje X</param>
        /// <param name="scale">Escala del ruido</param>
        /// <param name="offsetX">Desplazamiento en X</param>
        /// <param name="offsetZ">Desplazamiento en Z</param>
        /// <param name="waves">Waves a usar para meter mas ruido</param>
        /// <returns>Mapa de ruido generado</returns>
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
        /// Procesa los valores de los distintos mapas.
        /// Estos valores se influyen entre si segun ciertas formulas.
        /// </summary>
        /// <param name="heightMap">Mapa de alturas</param>
        /// <param name="humidityMap">Mapa de humedad</param>
        /// <param name="temperatureMap">Mapa de temeperatura</param>
        /// <returns>Mapa fisico generado</returns>
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
                        mapData[xIndex, yIndex].temperature = temperatureMap[xIndex, yIndex] - evaluation/2;
                        mapData[xIndex, yIndex].humidity = humidityMap[xIndex, yIndex] + evaluation;
                        if (mapData[xIndex, yIndex].temperature < 0) mapData[xIndex, yIndex].temperature = 0; // Para que no se salga del dominio
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
        /// Genera todo el mapa de manera aletoria.
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
        /// Evalua un valor segun la funcion de altura
        /// </summary>
        /// <param name="x">Valor a evaluar</param>
        /// <returns>Valor resultante</returns>
        double EvaluateHeightCurve(double x)
        {
            if (x < 0) return 0f;
            else if (x < 0.3) return (1 / 4f) * Math.Sin((10 / 3) * Math.PI * (x - 0.15)) + 0.25f;
            else if (x < 0.5) return 0.5f;
            else if (x < 1) return (1 / 4f) * Math.Sin(2 * Math.PI * (x - 0.75f)) + 0.75f;
            else return 1;
        }

        /// <summary>
        /// Evalua un valor segun la funcion de influencia
        /// </summary>
        /// <param name="x">Valor a evaluar</param>
        /// <returns>Valor resultante</returns>
        double EvaluateInfluenceCurve(double x)
        {
            if (x < 0) return -1;
            else if (x < 0.5) return Math.Pow((2 * x), 2) - 1;
            else if (x < 1) return 2 * (x - 0.5);
            else return 1;
        }
        #endregion

        // Entidades en el mundo
        public List<IEntity> entities { get; private set; }
        // Entidades a eliminar
        List<IEntity> delete;
        // Mapa fisico
        public MapData[,] map{ get; private set; }
        // Tamanio del mapa
        int mapSize;
        // Generador de rudio de Perlin
        Perlin p;
    }
}
