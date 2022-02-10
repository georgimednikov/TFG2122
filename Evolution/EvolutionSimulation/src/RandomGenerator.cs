using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public static class RandomGenerator
    {
        static Random r = new Random();

        public static int Next()
        {
            return r.Next();
        }

        public static int Next(int minValue, int maxValue)
        {
            return r.Next(minValue, maxValue);
        }

        public static int Next(int maxValue)
        {
            return r.Next(maxValue);
        }

        public static double NextDouble()
        {
            return r.NextDouble();
        }

        public static void NextByte(ref byte[] bytes)
        {
            r.NextBytes(bytes);
        }
    }
}