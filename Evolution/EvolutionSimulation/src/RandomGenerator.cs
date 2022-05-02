using System;

namespace EvolutionSimulation
{
    public class RandomGenerator
    {
        static Random r = new Random();

        /// <summary>
        /// Returns a int between 0 and the max Integer possible
        /// </summary>
        public static int Next()
        {
            return r.Next();
        }

        /// <summary>
        /// Returns a int between the interval [minValue, maxValue)
        /// </summary>
        public static int Next(int minValue, int maxValue)
        {
            return r.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a int between the interval [0, maxValue)
        /// </summary>
        public static int Next(int maxValue)
        {
            return r.Next(maxValue);
        }

        /// <summary>
        /// Returns a double between the interval [0.0, 1.0)
        /// </summary>
        public static double NextDouble()
        {
            return r.NextDouble();
        }

        /// <summary>
        /// Fills a byte with random bits
        /// </summary>
        public static void NextByte(ref byte[] bytes)
        {
            r.NextBytes(bytes);
        }
    }
}